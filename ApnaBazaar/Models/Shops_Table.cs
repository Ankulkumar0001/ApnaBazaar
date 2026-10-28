using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaBazaar.Models
{
    public class Shops_Table
    {
        [Key]
        public int shop_id { get; set; }
        public int merchant_id { get; set; }
        public string shopname { get; set; }
        public string shop_category { get; set; }
        public string? shop_tagline { get; set; }
        public string? shop_manager_name { get; set; }
        public string mobile { get; set; }
        public string whatsapp_number { get; set; }
        public string shop_email { get; set; }
        public string upi_id { get; set; }
        public string? shop_gstin_number { get; set; }
        public string shop_full_address { get; set; }
        public string shop_landmark { get; set; }
        public string shop_pincode { get; set; }
        public string shop_city { get; set; }
        public string home_delivery_radius { get; set; }
        public string shop_opening_time { get; set; }
        public string shop_closing_time { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal shop_latitude { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal shop_longitude { get; set; }
        public bool isVerified { get; set; }
        public bool isOpen { get; set; }
        public string? image {  get; set; }

        [NotMapped]
        public IFormFile? file { get; set; }
        [NotMapped]
        public double? distance { get; set; }
    }
}
