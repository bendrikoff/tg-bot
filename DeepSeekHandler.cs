using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Misha_s_bot.DeepSeek
{
    public class DeepSeekHandler
    {
        private readonly string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private static readonly string apiKey = Environment.GetEnvironmentVariable("DEEP_SEEK_TOKEN"); // Замените на свой API-ключ

        public async Task<string> HandleMessage(string userMessage)
        {
            try
            {
                var startPrompt = 
                    "Я пишу текст для книги. " +
               "Мне нужна твоя помощь в диалоге." +
               " Я пишу диалог двух молодых людей." +
               " Один из них всегда злой и грубый, на любой ответ он отвечает грубостью, что бы у него не спросили." +
               " Он любит анекдоты, любит сидеть дома и играть в стратегию, еще ты любишь девочку по имени Жужа. Отвечай как он." +
               " Будь грубым. Отвечай всегда грубо и используй обзывательства, что бы у тебя не  спросили. " +
               " Единственное, если попросят рассказать анекдот то расскажи анекдот. " +
               "Тебя зовут Миша. Ипользуй простую разговорную лексику. Отвечай по русски. Я пишу вопрос, ты ответ, без раасуждений и лишнего текста, тольк ответ";

                Console.WriteLine("Отправлено сообщение DeepSeek: " + userMessage);
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    string jsonData = $@"{{
                        ""model"": ""deepseek/deepseek-chat:free"",
                        ""messages"": [
                            {{ ""role"": ""system"", ""content"": ""{startPrompt}"" }},
                            {{ ""role"": ""user"", ""content"": ""{userMessage}"" }}
                        ]
                        }}";



                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    return await response.Content.ReadAsStringAsync();
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<string> DeserializeAsync(Task<string> jsonTask)
        {
            string json = await jsonTask; // Ждём результат задачи

            JObject jObject = JObject.Parse(json);
            var message = jObject["choices"]?[0]?["message"]?.ToObject<Message>();

            return message?.Content ?? string.Empty; // Возвращаем результат
        }


        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }
}
