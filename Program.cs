using System;
using System.Text;
using System.Text.Json;

namespace httpSMSBatch
{
    public class Message
    {
        public string from { get; set; }
        public List<string> phoneNumbers { get; set; }
        public string message { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            CommandLineParser clp = CommandLineParser.Parse();
            string json = clp.GetString("json");
            string apiKey = clp.GetEnv("httpSendApiKey", "httpSendApiKey");
            string apiEndpoint = clp.GetString("apiEndpoint", "https://api.httpsms.com/v1/messages/send");

            Message message = JsonSerializer.Deserialize<Message>(System.IO.File.ReadAllText(json));

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            HashSet<string> phoneNumbers = new HashSet<string>();
            foreach (string phoneNumber in message.phoneNumbers)
            {
                if (!phoneNumbers.Contains(phoneNumber))
                {
                    var t = Task.Run(async () =>
                    {
                        var response = await client.PostAsync(
                            apiEndpoint,
                            new StringContent(
                            JsonSerializer.Serialize(new
                            {
                                from = message.from,
                                To = phoneNumber,
                                Content = message.message,
                            }),
                            Encoding.UTF8,
                            "application/json"));
                    });
                    Task.WaitAll(t);
                    phoneNumbers.Add(phoneNumber);
                }
                else
                {
                    Console.WriteLine($"Skipping duplicate phone number {phoneNumber}");
                }
            }
        }
    }
}