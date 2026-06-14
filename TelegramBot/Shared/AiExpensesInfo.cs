using System.Text.Json.Serialization;

namespace TelegramBot.Shared;

public class AiExpensesInfo
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CategoryType ExpensesCategory { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Amount { get; set; }

}
