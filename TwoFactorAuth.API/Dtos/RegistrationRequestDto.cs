﻿namespace TwoFactorAuth.API.Dtos
{
    public class RegistrationRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; } 
        public required string FirstName { get; set; } 
        public required string LastName { get; set; } 
    }
}
