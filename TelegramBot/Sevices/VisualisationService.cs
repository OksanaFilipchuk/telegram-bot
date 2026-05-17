using System.Text;
using TelegramBot.Shared;

namespace TelegramBot.Sevices;

public class VisualisationService
{
    private static LocalizationServise _localizationService = new LocalizationServise();
    public static string GetExpanseReportMessage(List<ExpenseReport> expenses, Language lang)
    {
        StringBuilder sb = new StringBuilder();

        if (expenses.Count() > 0)
        {

            foreach (var item in expenses)
            {
                sb.AppendLine($"<b>{_localizationService.Get(item.Category, lang)}</b> — {item.Total} грн - {item.Percentage} %");
            }
            return sb.ToString();
            ;
        }
        return _localizationService.Get("NoExpenses", lang);

    }
}
