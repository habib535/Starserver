using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StarServer
{
    public class StarServerHttpClient
    {
        private HttpClient _httpClient;

        public StarServerHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> PostAsync(string text)
        {
            var message = new TextMessage
            {
                Text = text
            };
            var requestJson = SerializeJson(message);
            using (var httpRequestMessage = CreateRequestMessage(HttpMethod.Post, requestJson))
            using (var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage))
            {
                return "Sent";
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string content)
        {
            var url = ConfigurationManager.AppSettings["SlackURL"];
            var requestMessage = new HttpRequestMessage(method, new Uri(url));
            requestMessage.Headers.Add("Accept", "application/json");
            if (content != null)
            {
                requestMessage.Content = GetStringContent(content);
            }
            return requestMessage;
        }

        private string SerializeJson<T>(T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            return JsonConvert.SerializeObject(value, jsonSerializerSettings);
        }

        private StringContent GetStringContent(string content)
        {
            return new StringContent(content, Encoding.UTF8, "application/json");
        }

        internal class TextMessage
        {
            public string Text { get; set; }
        }
    }
}
