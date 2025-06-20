﻿namespace TwoFactorAuth.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<FaceEmbedding> FaceEmbeddings { get; set; } = [];
        public List<FCMToken> FCMTokens { get; set; } = [];
    }
}
