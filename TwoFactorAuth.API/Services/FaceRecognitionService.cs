using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TwoFactorAuth.API.Contextes;
using TwoFactorAuth.API.Dtos;
using TwoFactorAuth.API.Models;

namespace TwoFactorAuth.API.Services
{
    public class FaceRecognitionService
    {
        private readonly AppDbContext _db;

        public FaceRecognitionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<float[]> GetEmbeddingByImage(byte[] file)
        {
            using var httpClient = new HttpClient();

            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(file);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            content.Add(fileContent, "file", "image.jpg");

            // отправка запроса на fastAPI, где считывается вектор лица
            var response = await httpClient.PostAsync("http://localhost:8000/face-vector", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Десериализация float-массива из JSON-строки
            var result = System.Text.Json.JsonSerializer.Deserialize<float[]>(json);

            return result!;
        }

        public async Task Registration(string email, byte[] file)
        {
            var user = await _db.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user !=null)
            {
                var embeddings = await GetEmbeddingByImage(file);
                await _db.FaceEmbeddings.AddAsync(new FaceEmbedding()
                {
                    UserId = user.Id,
                    Embedding = embeddings
                });
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckFace(string email, byte[] file)
        {
            var user = await _db.Users.FirstOrDefaultAsync(m => m.Email == email);
            if (user != null)
            {
                var embeddings = await _db.FaceEmbeddings.FirstOrDefaultAsync(m => m.UserId == user.Id);
                var embeddingsFromFile = await GetEmbeddingByImage(file);

                if(embeddings != null)
                {
                    var similarity = CosineSimilarity(embeddingsFromFile, embeddings.Embedding);

                    if(similarity > 0.6)
                    {
                        return true;
                    } 
                }
            }

            return false;
        }


        public static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of same length");

            double dot = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dot += vectorA[i] * vectorB[i];
                normA += Math.Pow(vectorA[i], 2);
                normB += Math.Pow(vectorB[i], 2);
            }

            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
