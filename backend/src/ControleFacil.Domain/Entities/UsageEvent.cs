using ControleFacil.Domain.Enums;

namespace ControleFacil.Domain.Entities;

public class UsageEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public UsageEventType EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
