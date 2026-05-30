using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Endpoints;
using ConstructionAssetAPI.Entities;
using ConstructionAssetAPI.Enums;
using ConstructionAssetAPI.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Services;

public class AssignmentService
{
    private readonly AppDbContext _db;

    public AssignmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AssignmentResponse>> GetAllAsync()
        => await _db.Assignments
            .Include(a => a.Equipment)
            .Include(a => a.JobSite)
            .Select(a => new AssignmentResponse(
                a.Id,
                a.EquipmentId,
                a.Equipment.Name,
                a.JobSiteId,
                a.JobSite.Name,
                a.AssignedDate,
                a.ReturnDate,
                a.Notes))
            .ToListAsync();

    public async Task<AssignmentResponse> CreateAsync(AssignmentInput input)
    {
        // FK existence
        var equipment = await _db.Equipment.FindAsync(input.EquipmentId)
            ?? throw new NotFoundException($"Equipment {input.EquipmentId} does not exist.");

        var jobSite = await _db.JobSites.FindAsync(input.JobSiteId)
            ?? throw new NotFoundException($"JobSite {input.JobSiteId} does not exist.");

        // Double-booking: an open assignment has ReturnDate == null
        var openAssignment = await _db.Assignments
            .Include(a => a.JobSite)
            .FirstOrDefaultAsync(a =>
                a.EquipmentId == input.EquipmentId &&
                a.ReturnDate == null);

        if (openAssignment is not null)
        {
            throw new ConflictException(
                $"Equipment '{equipment.Name}' (#{equipment.SerialNumber}) " +
                $"is already assigned to site '{openAssignment.JobSite.Name}' " +
                $"(assignment {openAssignment.Id}).");
        }

        var assignment = new Assignment
        {
            EquipmentId = input.EquipmentId,
            JobSiteId = input.JobSiteId,
            AssignedDate = input.AssignedDate,
            Notes = input.Notes
        };

        equipment.Status = EquipmentStatus.InUse;
        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        return new AssignmentResponse(
            assignment.Id,
            assignment.EquipmentId,
            equipment.Name,
            assignment.JobSiteId,
            jobSite.Name,
            assignment.AssignedDate,
            assignment.ReturnDate,
            assignment.Notes);
    }

    // Soft return: ReturnDate = now, equipment flipped back to Available.
    // Row is kept so site history survives.
    public async Task ReturnAsync(int id)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Equipment)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Assignment {id} not found.");

        if (assignment.ReturnDate is not null)
            throw new ConflictException($"Assignment {id} has already been returned.");

        assignment.ReturnDate = DateTime.UtcNow;
        assignment.Equipment.Status = EquipmentStatus.Available;
        await _db.SaveChangesAsync();
    }
}