using ConstructionAssetAPI.Enums;

namespace ConstructionAssetAPI.Entities;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    public DateTime? NextMaintenanceDate { get; set; }

    // Navigation property — one Equipment has many Assignments over time
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}