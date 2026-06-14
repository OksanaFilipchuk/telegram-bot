
using OllamaSharp;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TelegramBot.Shared;

namespace TelegramBot.Sevices;

public class AiOllamaProvider : IAiProvider
{
    private Uri Uri = new Uri("http://localhost:11434");
    string SelectedModel = "llama3";
    private OllamaApiClient _OllamaApiClient;
    private readonly Chat _Chat;

    public AiOllamaProvider()
    {
        _OllamaApiClient = new OllamaApiClient(Uri);
        _OllamaApiClient.SelectedModel = SelectedModel;
    }

    public async Task<AiExpensesInfo?> GetExpensesResponse(string prompt)
    {
        var systemPrompt = $$"""
                You are an expense parser.

                Your task is to extract an expense amount and category from the message

                Valid categories:
                {{string.Join(", ", Enum.GetNames<CategoryType>())}}

                Return ONLY one of the following:

                1. Valid JSON:

                {
                  "Amount": string,
                  "ExpensesCategory": "CategoryName"
                }

                2. null

                Rules:
                - Return JSON only.
                - Do not wrap JSON in markdown.
                - Do not add explanations.
                - Amount must be numeric. Can be negative
                - Category must be one of the valid categories.
                - If the message does not contain an expense, return null.
                - If the amount is unclear, return null.
                - If the category cannot be determined, choose the closest valid category.

                Examples:

                User: coffee 120
                Response:
                {"Amount":"120","ExpensesCategory":"Groceries"}

                User: paid 850 for taxi
                Response:
                {"Amount":"850","ExpensesCategory":"Transport"}

                User: bought groceries for 560
                Response:
                {"Amount":"560","ExpensesCategory":"Groceries"}

                User: remove groceries 560
                Response:
                {"Amount":"-560","ExpensesCategory":"Groceries"}

                User: hello how are you
                Response:
                null

                Message:{{prompt}}
                """;
        //var request = _Chat.SendAsync(prompt);
        var request = _OllamaApiClient.GenerateAsync(systemPrompt);
        var sb = new StringBuilder();


        await foreach (var token in request)
        {
            var tokenText = token?.Response;
            sb.Append(tokenText);
            Console.Write(tokenText);
        }

        AiExpensesInfo result = null;
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                    {
                        new JsonStringEnumConverter()
                    }
            };
            string text = sb.ToString();
            result = JsonSerializer.Deserialize<AiExpensesInfo>(text);
        }
        catch (Exception ex)
        {

        }
        return result;

    }
}
