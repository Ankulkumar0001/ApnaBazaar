using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ApnaBazaar.Models
{
    public class DatabaseConnection : DbContext
    {
        public DatabaseConnection(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserTable> UserTable { get; set; }
        public DbSet<MerchantTable> MerchantTable { get; set; }
        public DbSet<Shops_Table> Shops_Table { get; set; }
        public DbSet<Product_Table> Product_Table { get; set; }
    }
}
