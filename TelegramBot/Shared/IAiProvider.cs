namespace TelegramBot.Shared;

public interface IAiProvider
{
    Task<AiExpensesInfo> GetExpensesResponse(string prompt);
}
