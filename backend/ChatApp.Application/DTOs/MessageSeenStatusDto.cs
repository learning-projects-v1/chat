namespace ChatApp.Application.DTOs;

public class MessageSeenStatusDto
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime SeenAt { get; set; }
}
