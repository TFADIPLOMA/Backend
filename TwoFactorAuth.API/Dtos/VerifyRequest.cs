﻿namespace TwoFactorAuth.API.Dtos
{
    public class VerifyRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
