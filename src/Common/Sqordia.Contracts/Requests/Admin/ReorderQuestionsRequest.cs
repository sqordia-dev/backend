namespace Sqordia.Contracts.Requests.Admin;

public class ReorderQuestionsRequest
{
    public required List<QuestionOrderItem> Items { get; set; }
}

public class QuestionOrderItem
{
    public required Guid QuestionId { get; set; }
    public required int Order { get; set; }
}
