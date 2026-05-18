using ChurchLearn.Api.Domain.Enums;

namespace ChurchLearn.Api.Domain.Entities;

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContentType ContentType { get; set; }
    public string? YouTubeUrl { get; set; }
    public string? TextContent { get; set; }
    public string? PdfUrl { get; set; }
    public int DurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPreview { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public Quiz? Quiz { get; set; }
}
