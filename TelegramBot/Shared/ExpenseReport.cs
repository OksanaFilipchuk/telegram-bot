namespace TelegramBot.Shared;

public struct ExpenseReport
{
    public string Category { get; set; }
    public decimal Total { get; set; }
    public decimal Percentage { get; set; }
}
