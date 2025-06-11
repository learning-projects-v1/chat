namespace ChatApp.Application.DTOs;

public class LatestMessageDto
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Message { get; set; }
    public DateTime MessageTime { get; set; }
    public Guid? MessageSenderId { get; set; }
}
