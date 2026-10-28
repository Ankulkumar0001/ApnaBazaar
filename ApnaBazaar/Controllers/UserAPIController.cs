using ApnaBazaar.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApnaBazaar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        DatabaseConnection connection;
        public UserAPIController(DatabaseConnection connection)
        {
            this.connection = connection;
        }

        [HttpPatch("updateUserLocation")]
        public IActionResult UpdateLocation([FromForm] UpdateUserLocation user)
        {
            try
            {
                var data = connection.UserTable.Find(user.id);
                data.latitude = user.latitude;
                data.longitude = user.longitude;
                connection.UserTable.Update(data);
                connection.SaveChanges();
                BasicResponse response = new BasicResponse();
                response.status = "Success";
                response.message = "Location Update Successfully";
                response.data = data;
                response.data_second = null;
                return Ok(response);

            }catch (Exception ex)
            {
                BasicResponse response = new BasicResponse();
                response.status = "Failed";
                response.message = "Location Not Update";
                response.data = null;
                response.data_second = null;
                return BadRequest(response);
            }

        }



        [HttpGet("SearchNearby")]
        public async Task<IActionResult> SearchNearby(
            int id,
            string shopCategory,
            double distanceKm)
        {
            if (string.IsNullOrWhiteSpace(shopCategory))
            {
                return BadRequest(new BasicResponse
                {
                    status = "error",
                    message = "Please enter a search term",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            if (distanceKm <= 0 || distanceKm > 100)
            {
                return BadRequest(new BasicResponse
                {
                    status = "error",
                    message = "Distance must be between 0 and 100 km",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            // Get user location
            var user = await connection.UserTable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == id);

            if (user == null)
            {
                return NotFound(new BasicResponse
                {
                    status = "error",
                    message = "User not found",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            if (user.latitude == null || user.longitude == null)
            {
                return BadRequest(new BasicResponse
                {
                    status = "error",
                    message = "User location not available",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            double userLat = (double)user.latitude;
            double userLon = (double)user.longitude;
            string search = shopCategory.Trim();

            // Get verified and open shops
            var allShops = await connection.Shops_Table
                .AsNoTracking()
                .Where(s =>
                    s.isVerified == true &&
                    s.isOpen == true &&
                    s.shop_category != null &&
                    s.shop_latitude != null &&
                    s.shop_longitude != null)
                .ToListAsync();

            // Calculate distance and keep nearby shops
            var nearbyShops = allShops
                .Select(s =>
                {
                    double shopLat = (double)s.shop_latitude;
                    double shopLon = (double)s.shop_longitude;

                    double dLat = (shopLat - userLat) * Math.PI / 180;
                    double dLon = (shopLon - userLon) * Math.PI / 180;

                    double a =
                        Math.Pow(Math.Sin(dLat / 2), 2) +
                        Math.Cos(userLat * Math.PI / 180) *
                        Math.Cos(shopLat * Math.PI / 180) *
                        Math.Pow(Math.Sin(dLon / 2), 2);

                    a = Math.Max(0, Math.Min(1, a));

                    double c = 2 * Math.Atan2(
                        Math.Sqrt(a),
                        Math.Sqrt(1 - a));

                    // Distance in kilometers
                    double calculatedDistance = 6371 * c;

                    return new
                    {
                        Shop = s,
                        DistanceKm = calculatedDistance
                    };
                })
                .Where(x => x.DistanceKm <= distanceKm)
                .ToList();

            // Assign distance to the shop's NotMapped property
            foreach (var item in nearbyShops)
            {
                item.Shop.distance = Math.Round(item.DistanceKm, 2);
            }

            // Search shop category first
            var shops = nearbyShops
                .Where(x => x.Shop.shop_category.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Shop)
                .ToList();

            List<Product_Table> products;

            if (shops.Count > 0)
            {
                // Category matched: fetch products from matching shops
                var shopIds = shops.Select(s => s.shop_id).ToList();

                products = await connection.Product_Table
                    .AsNoTracking()
                    .Where(p => shopIds.Contains(p.shop_id))
                    .ToListAsync();
            }
            else
            {
                // No category match: search product name, category and description
                products = await connection.Product_Table
                    .AsNoTracking()
                    .Where(p =>
                        (p.product_name != null &&
                         p.product_name.Contains(search)) ||
                        (p.product_category != null &&
                         p.product_category.Contains(search)) ||
                        (p.product_discription != null &&
                         p.product_discription.Contains(search)))
                    .ToListAsync();

                // Keep products belonging to nearby shops only
                var nearbyShopIds = nearbyShops
                    .Select(x => x.Shop.shop_id)
                    .ToList();

                products = products
                    .Where(p => nearbyShopIds.Contains(p.shop_id))
                    .ToList();

                // Return matching shops with their calculated distance
                var matchingShopIds = products
                    .Select(p => p.shop_id)
                    .Distinct()
                    .ToList();

                shops = nearbyShops
                    .Where(x => matchingShopIds.Contains(x.Shop.shop_id))
                    .Select(x => x.Shop)
                    .ToList();
            }

            return Ok(new BasicResponse
            {
                status = "success",
                message = shops.Count == 0 && products.Count == 0
                    ? "No matching shops or products found nearby"
                    : "Nearby search completed successfully",
                data = shops,
                data_second = products
            });
        }

        [HttpGet("GetNearbyShops")]
        public async Task<IActionResult> GetNearbyShops(int id, double distanceKm)
        {
            // 1. Validate distance
            if (distanceKm <= 0 || distanceKm > 100)
            {
                return BadRequest(new BasicResponse
                {
                    status = "error",
                    message = "Distance must be between 0 and 100 km",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            // 2. Find user
            var user = await connection.UserTable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == id);

            if (user == null)
            {
                return NotFound(new BasicResponse
                {
                    status = "error",
                    message = "User not found",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            // 3. Check user location
            if (user.latitude == null || user.longitude == null)
            {
                return BadRequest(new BasicResponse
                {
                    status = "error",
                    message = "User location is not available",
                    data = new List<Shops_Table>(),
                    data_second = new List<Product_Table>()
                });
            }

            double userLat = (double)user.latitude;
            double userLon = (double)user.longitude;

            // 4. Fetch verified and open shops
            var allShops = await connection.Shops_Table
                .AsNoTracking()
                .Where(s =>
                    s.isVerified == true &&
                    s.isOpen == true &&
                    s.shop_latitude != null &&
                    s.shop_longitude != null)
                .ToListAsync();

            // 5. Calculate distance using Haversine formula
            var nearbyShops = allShops
                .Select(s =>
                {
                    double shopLat = (double)s.shop_latitude;
                    double shopLon = (double)s.shop_longitude;

                    double dLat = (shopLat - userLat) * Math.PI / 180;
                    double dLon = (shopLon - userLon) * Math.PI / 180;

                    double a =
                        Math.Pow(Math.Sin(dLat / 2), 2) +
                        Math.Cos(userLat * Math.PI / 180) *
                        Math.Cos(shopLat * Math.PI / 180) *
                        Math.Pow(Math.Sin(dLon / 2), 2);

                    a = Math.Max(0, Math.Min(1, a));

                    double c = 2 * Math.Atan2(
                        Math.Sqrt(a),
                        Math.Sqrt(1 - a));

                    double calculatedDistance = 6371 * c;

                    return new
                    {
                        Shop = s,
                        Distance = calculatedDistance
                    };
                })
                // Filter shops according to requested radius
                .Where(x => x.Distance <= distanceKm)
                // Nearest shops first
                .OrderBy(x => x.Distance)
                .ToList();

            // 6. Assign distance to every shop
            foreach (var item in nearbyShops)
            {
                item.Shop.distance = Math.Round(item.Distance, 2);
            }

            var shops = nearbyShops
                .Select(x => x.Shop)
                .ToList();

            // 7. Get products belonging to nearby shops
            var shopIds = shops
                .Select(s => s.shop_id)
                .ToList();

            var products = await connection.Product_Table
                .AsNoTracking()
                .Where(p => shopIds.Contains(p.shop_id))
                .ToListAsync();

            foreach (var p in products)
            {
                var shop = connection.Shops_Table.Find(p.shop_id);
                p.shopname = shop.shopname;
            }

            // 8. Return shops and products
            return Ok(new BasicResponse
            {
                status = "success",
                message = shops.Count == 0
                    ? "No nearby shops found"
                    : "Nearby shops and products fetched successfully",

                data = shops,
                data_second = products
            });
        }



    }
}
