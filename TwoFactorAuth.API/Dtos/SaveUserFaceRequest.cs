namespace TwoFactorAuth.API.Dtos
{
    public class SaveUserFaceRequest
    {
        public string Email {  get; set; }
        public IFormFile File {  get; set; }
    }
}
