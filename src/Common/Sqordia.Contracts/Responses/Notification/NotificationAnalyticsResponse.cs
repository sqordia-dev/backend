namespace Sqordia.Contracts.Responses.Notification;

public class NotificationAnalyticsResponse
{
    public int TotalSent { get; set; }
    public int TotalRead { get; set; }
    public double ReadRate { get; set; }
    public double AverageTimeToReadMinutes { get; set; }
    public int ActiveUsersWithNotifications { get; set; }
    public List<NotificationTypeStats> ByType { get; set; } = new();
    public List<NotificationPriorityStats> ByPriority { get; set; } = new();
    public List<NotificationDailyStats> DailyTrend { get; set; } = new();
}

public class NotificationTypeStats
{
    public string Type { get; set; } = null!;
    public int Sent { get; set; }
    public int Read { get; set; }
    public double ReadRate { get; set; }
}

public class NotificationPriorityStats
{
    public string Priority { get; set; } = null!;
    public int Sent { get; set; }
    public int Read { get; set; }
    public double ReadRate { get; set; }
}

public class NotificationDailyStats
{
    public string Date { get; set; } = null!;
    public int Sent { get; set; }
    public int Read { get; set; }
}
