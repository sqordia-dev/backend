namespace Sqordia.Contracts.Responses.Notification;

public class NotificationListResponse
{
    public List<NotificationResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
}
