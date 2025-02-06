using BusinessLogicLayer.Interface.Microservices_Interface.OpenAI;
using OpenAI.Chat;


namespace BusinessLogicLayer.Implement.Microservices.OpenAI
{
    public class OpenAIService : IOpenAIService
    {
        public async Task TestOpenApi()
        {
            ChatClient client = new(model: "gpt-3.5-turbo", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            // Tạo danh sách các tin nhắn
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("You are an assistant that provides structured answers in JSON format."),
                ChatMessage.CreateUserMessage(@"
                    For every question, return a JSON response with the following structure:
                    {
                      ""Question"": ""The input question"",
                      ""Answer"": ""A detailed and accurate answer"",
                      ""Source"": ""A relevant source or 'N/A' if unavailable""
                    }

                    Now, answer the following question:
                    What is the capital of France?")
            };

            // Cấu hình ChatCompletionOptions
            ChatCompletionOptions chatOptions = new()
            {
                MaxOutputTokenCount = 20, // Tối đa token trả về
                Temperature = (float?)0.2 // Tăng độ chính xác
            };

            // Gửi yêu cầu tới GPT
            ChatCompletion response = await client.CompleteChatAsync(messages, chatOptions);

            // Xử lý phản hồi JSON từ GPT
            ChatMessageContent jsonResponse = response.Content;
            Console.WriteLine("GPT Response: " + jsonResponse);

            //// Giải mã JSON thành object nếu cần
            //try
            //{
            //    var structuredResponse = JsonSerializer.Deserialize<YourResponseModel>(jsonResponse);
            //    Console.WriteLine($"Question: {structuredResponse.Question}");
            //    Console.WriteLine($"Answer: {structuredResponse.Answer}");
            //    Console.WriteLine($"Source: {structuredResponse.Source}");
            //}
            //catch (JsonException ex)
            //{
            //    Console.WriteLine("Failed to parse JSON: " + ex.Message);
            //}
        }
    }
}
