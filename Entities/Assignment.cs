namespace ConstructionAssetAPI.Entities;

public class Assignment
{
    public int Id {get; set;}

    //Foreign Keys
    public int EquipmentId { get; set;}
    public int JobSiteId {get; set;}

    public DateTime AssignedDate {get; set;}
    public DateTime? ReturnDate {get; set;}
    public string? Notes {get; set;}

    //Navigation properties
    public Equipment Equipment {get; set;} = null!;
    public JobSite JobSite {get; set;} = null!;
}