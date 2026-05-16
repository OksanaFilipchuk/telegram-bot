using System.Text.RegularExpressions;

namespace TelegramBot.Extention;

public static class Common
{
    extension(string str)
    {
        public decimal? ParseAmount()
        {
            var match = Regex.Match(str, @"\d+(\.\d+)?");

            if (!match.Success)
                return null;

            if (decimal.TryParse(match.Value, out var result))
                return result;

            return null;
        }
    }
}
