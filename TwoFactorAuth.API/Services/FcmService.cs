using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace TwoFactorAuth.API.Services
{
    public class FcmService
    {
       

        private readonly IWebHostEnvironment _env;
        private readonly HttpClient _httpClient;
        private readonly string _projectId = "tfadiploma";

        public FcmService(IWebHostEnvironment env)
        {
            _env = env;
            _httpClient = new HttpClient();
        }

        public async Task SendNotificationAsync(string deviceToken, string title, string body)
        {

            string filePath = Path.Combine(_env.WebRootPath, "fcmserviceaccount.json");

            var credential = Google.Apis.Auth.OAuth2.GoogleCredential
            .FromFile(filePath)
            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var payload = new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new
                    {
                        title,
                        body
                    }
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Устанавливаем заголовки
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Отправляем запрос
            var fcmUrl = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
            var response = await _httpClient.PostAsync(fcmUrl, content);

            // Читаем ответ
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"FCM status: {response.StatusCode}");
            Console.WriteLine($"Response: {responseBody}");

            response.EnsureSuccessStatusCode();
        }
    }
}
