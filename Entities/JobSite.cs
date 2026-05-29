namespace ConstructionAssetAPI.Entities;

public class JobSite
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Navigation property — one JobSite has many Assignments
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}