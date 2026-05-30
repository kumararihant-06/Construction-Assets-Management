using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Endpoints;
using ConstructionAssetAPI.Entities;
using ConstructionAssetAPI.Enums;
using ConstructionAssetAPI.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ConstructionAssetAPI.Services;

public class EquipmentService
{
    private readonly AppDbContext _db;

    public EquipmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Equipment>> GetAllAsync()
        => await _db.Equipment.ToListAsync();

    public async Task<Equipment> GetByIdAsync(int id)
    {
        var equipment = await _db.Equipment.FindAsync(id)
            ?? throw new NotFoundException($"Equipment {id} not found.");
        return equipment;
    }

    public async Task<Equipment> CreateAsync(EquipmentInput input)
    {
        var equipment = new Equipment
        {
            Name = input.Name,
            Type = input.Type,
            SerialNumber = input.SerialNumber,
            Status = input.Status,
            NextMaintenanceDate = input.NextMaintenanceDate
        };

        _db.Equipment.Add(equipment);
        await _db.SaveChangesAsync();
        return equipment;
    }

    public async Task UpdateAsync(int id, EquipmentInput input)
    {
        var equipment = await _db.Equipment.FindAsync(id)
            ?? throw new NotFoundException($"Equipment {id} not found.");

        equipment.Name = input.Name;
        equipment.Type = input.Type;
        equipment.SerialNumber = input.SerialNumber;
        equipment.Status = input.Status;
        equipment.NextMaintenanceDate = input.NextMaintenanceDate;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var equipment = await _db.Equipment.FindAsync(id)
            ?? throw new NotFoundException($"Equipment {id} not found.");

        _db.Equipment.Remove(equipment);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Equipment>> GetAvailableAsync()
        => await _db.Equipment
            .Where(e => e.Status == EquipmentStatus.Available)
            .ToListAsync();

    public async Task<List<Equipment>> GetMaintenanceDueAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(7);
        return await _db.Equipment
            .Where(e => e.NextMaintenanceDate <= cutoff)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, EquipmentStatus status)
    {
        var equipment = await _db.Equipment.FindAsync(id)
            ?? throw new NotFoundException($"Equipment {id} not found.");

        equipment.Status = status;
        await _db.SaveChangesAsync();
    }
}