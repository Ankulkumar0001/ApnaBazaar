using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaBazaar.Models
{
    public class UserTable
    {
        [Key]
        public int id { get; set; }
        public string name  { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string password { get; set; }
        public string? gender { get; set; }
        public string? image { get; set; }

        [NotMapped]
        public string confirmPassword { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal? latitude { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal? longitude { get; set; }

        [NotMapped]
        public IFormFile? file { get; set; }
    }
}
