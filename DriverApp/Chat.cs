namespace DriverApp;
public class Chat
{
    public string Recipient { get; set; }
    public string Sender { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class RideChat
{
    public List<Chat> Chats { get; set; } = [];
}
public sealed class SendChatMessage
{
    public string Message { get; set; }
    public long RideId { get; set; }
}
