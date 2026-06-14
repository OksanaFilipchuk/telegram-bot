using System.Text.RegularExpressions;

namespace TelegramBot.Extention;

public static class Common
{
    extension(string str)
    {
        public decimal? ParseAmount()
        {
            var match = Regex.IsMatch(str, @"\d+[,.]?(\d+)?");
            if (!match)
                return null;

            if (decimal.TryParse(str, out var result))
                return result;

            return null;
        }
    }
}
