using Newtonsoft.Json;
using System.Text;

namespace TwoFactorAuth.API.Services
{
    public class FcmService
    {
        private readonly string _serverKey = "AIzaSyCdUmhWj_FXxpmLsz7JEfJ5zUPuPYcWSBw";
        private readonly string _fcmUrl = "https://fcm.googleapis.com/fcm/send";

        public async Task SendNotificationAsync(string deviceToken, string title, string body)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={_serverKey}");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            var message = new
            {
                to = deviceToken,
                notification = new
                {
                    title = title,
                    body = body
                },
                // Можно также отправлять дополнительные данные
                data = new
                {
                    customKey1 = "value1",
                    customKey2 = "value2"
                }
            };

            var jsonMessage = JsonConvert.SerializeObject(message);
            var httpContent = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_fcmUrl, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"FCM Error: {response.StatusCode} - {result}");
            }

            Console.WriteLine("Push sent successfully: " + result);
        }
    }
}
