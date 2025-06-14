namespace ChatApp.Application.DTOs;

public class LatestMessageDto
{
    public Guid FriendId { get; set; }
    public string FriendUsername { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? MessageSenderId { get; set; }
}
