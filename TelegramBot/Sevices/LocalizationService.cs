using System.Text.Json;
using TelegramBot.Shared;

namespace TelegramBot.Sevices;

public class LocalizationService
{
    Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();
    public LocalizationService()
    {
        Load();
    }
    private static Dictionary<string, string> langFileLocation = new Dictionary<string, string>(){
        [Language.UA.ToString().ToLower()] = "ua.json",
        [Language.EN.ToString().ToLower()] = "en.json"};

    public void Load() {
        foreach (var key in langFileLocation.Keys) {
            var path  = Path.Combine(AppContext.BaseDirectory, "Localization", langFileLocation[key]);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Localization file for language '{key}' not found at path: {langFileLocation[key]}");
            }
            string fileContent = File.ReadAllText(path);
            _data.Add(key.ToString().ToLower(), JsonSerializer.Deserialize<Dictionary<string, string>>(fileContent));
        }
    }

    public string Get(string key, Language lang = Language.UA)
    {
        if (_data.ContainsKey(lang.ToString().ToLower()) && _data[lang.ToString().ToLower()].TryGetValue(key, out var value))
        {
            return value;
        }
        return key;
    }
}