using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TwoFactorAuth.API.Models
{
    public class FaceEmbedding
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        [Required]
        [Column(TypeName = "real[]")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
