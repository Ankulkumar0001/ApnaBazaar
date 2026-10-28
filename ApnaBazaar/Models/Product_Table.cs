using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaBazaar.Models
{
    public class Product_Table
    {
        [Key]
        public int product_id { get; set; }
        public int shop_id { get; set; }
        public int merchant_id { get; set; }
        public string product_name { get; set; }
        public string product_category { get; set; }
        public string product_unit { get; set; }
        public int product_selling_price { get; set; }
        public int product_mrp { get; set; }
        public int product_stock { get; set; }
        public int product_lowstock { get; set; }
        public string product_discription { get; set; }
        public string product_image { get; set; }

        [NotMapped]
        public IFormFile file { get; set; }

        [NotMapped]
        public string shopname { get; set; }

    }
}
