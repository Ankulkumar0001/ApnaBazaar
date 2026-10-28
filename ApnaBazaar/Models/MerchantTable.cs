using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaBazaar.Models
{
    public class MerchantTable
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string password { get; set; }

        [NotMapped]
        public string confirmPassword { get; set; }
        public string? altMobile { get; set; }
        public string? aadharNumber { get; set; }
        public string? address { get; set; }
        public string? gender { get; set; }
        public string? dob { get; set; }
        public string? image { get; set; }
        public bool isVerified { get; set; }

        [NotMapped]
        public IFormFile? file { get; set; }
    }
}
