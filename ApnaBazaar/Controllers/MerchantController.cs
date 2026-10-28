using ApnaBazaar.Models;
using ApnaBazaar.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApnaBazaar.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class MerchantController : Controller
    {
        DatabaseConnection connection;
        IWebHostEnvironment env;
        EmailSender emailSender;
        public MerchantController(DatabaseConnection connection,IWebHostEnvironment env,EmailSender emailSender) {

            this.connection = connection;
            this.env = env;
            this.emailSender = emailSender;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (HttpContext.Session.GetString("merchant_id") == null)
            {
                context.Result = RedirectToAction("MerchantLogin", "Login");
            }
        }
        public async Task<IActionResult> Index()
        {
            int merchant_id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(merchant_id);
            ViewBag.isVerified = data.isVerified;
            ViewBag.Name = data.name;
            ViewBag.image = data.image;

            var shopdata = await connection.Shops_Table.Where(x => x.merchant_id == merchant_id).ToListAsync();
            DashBoardCombineModel dashBoard = new DashBoardCombineModel();
            dashBoard.ShopData = shopdata;

            return View(dashBoard);
        }

        public IActionResult VerifyAccount()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            if(data.isVerified == true)
            {
                return RedirectToAction("Index","Merchant");
            }
            return View(data);
        }

        [HttpPost]
        public IActionResult VerifyAadhar(MerchantTable data)
        {
            if(data.aadharNumber != null)
            {
                Random random = new Random();
                int otp = random.Next(1000,9999);

                HttpContext.Session.SetInt32("otp", otp);
                TempData["msg"] = "OTP Sent Successfully";
                return RedirectToAction("AadharOTP","Merchant");
            }
            else
            {
                TempData["nomsg"] = "Please Enter Aadhar Number for Verification.";
                return RedirectToAction("VerifyAccount","Merchant");
            }
        }

        public IActionResult AadharOTP()
        {
            if (HttpContext.Session.GetInt32("otp") == null)
            {
                return RedirectToAction("VerifyAccount", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyAadharOtp(ForEmailAndOTP data)
        {
            int? otp = HttpContext.Session.GetInt32("otp");
            if(data.otp == otp)
            {
                int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
                var mdata = connection.MerchantTable.Find(id);
                mdata.isVerified = true;
                connection.MerchantTable.Update(mdata);
                connection.SaveChanges();
                HttpContext.Session.Remove("otp");
                TempData["msg"] = ""+mdata.name+" Verified Successfully";
                return RedirectToAction("Index", "Merchant");
            }
            else
            {
                TempData["nomsg"] = "Invalid OTP!";
                return RedirectToAction("AadharOTP","Merchant");
            }
        }

        public async Task<IActionResult> AllOrder()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            ViewBag.Name = data.name;
            ViewBag.image = data.image;
            ViewBag.isVerified = data.isVerified;

            var shopdata = await connection.Shops_Table.Where(x => x.merchant_id == id).ToListAsync();
            OrderConbimeModel order = new OrderConbimeModel();
            order.ShopData = shopdata;
            return View(order);
        }

        public async Task<IActionResult> ProductAndStock(int? id)
        {
            int merchant_id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var mdata = connection.MerchantTable.Find(merchant_id);
            ViewBag.Name = mdata.name;
            ViewBag.image = mdata.image;
            ViewBag.isVerified = mdata.isVerified;

            ProductAndStockCombineModel stock = new ProductAndStockCombineModel();
            var shopdata = await connection.Shops_Table.Where(x => x.merchant_id == merchant_id).ToListAsync();
            stock.ShopData = shopdata;

            if (id == null)
            {
                var instockdata = await connection.Product_Table.Where(x => x.product_stock > 0 && x.merchant_id == merchant_id).ToListAsync();
                int instock = instockdata.Count();
                ViewBag.InStock = instock;

                var lowstockdata = await connection.Product_Table.Where(x => x.product_stock > 0 && x.product_stock <= x.product_lowstock && x.merchant_id == merchant_id).ToListAsync();
                int lowstock = lowstockdata.Count();
                ViewBag.LowStock = lowstock;

                var outofstockdata = await connection.Product_Table.Where(x => x.product_stock == 0 && x.merchant_id == merchant_id).ToListAsync();
                int outofstock = outofstockdata.Count();
                ViewBag.OutOfStock = outofstock;

                var productdata = await connection.Product_Table.Where(x => x.merchant_id == merchant_id).ToListAsync();

                stock.ProductData = productdata;
            }
            else
            {
                var instockdata = await connection.Product_Table.Where(x => x.product_stock > 0 && x.shop_id == id && x.merchant_id == merchant_id).ToListAsync();
                int instock = instockdata.Count();
                ViewBag.InStock = instock;

                var lowstockdata = await connection.Product_Table.Where(x => x.product_stock > 0 && x.product_stock <= x.product_lowstock && x.shop_id == id && x.merchant_id == merchant_id).ToListAsync();
                int lowstock = lowstockdata.Count();
                ViewBag.LowStock = lowstock;

                var outofstockdata = await connection.Product_Table.Where(x => x.product_stock == 0 && x.shop_id == id && x.merchant_id == merchant_id).ToListAsync();
                int outofstock = outofstockdata.Count();
                ViewBag.OutOfStock = outofstock;

                var productdata = await connection.Product_Table.Where(x => x.shop_id == id && x.merchant_id == merchant_id).ToListAsync();
                
                stock.ProductData = productdata;
            }

            return View(stock);
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            int merchant_id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var mdata = connection.MerchantTable.Find(merchant_id);
            ViewBag.isVerified = mdata.isVerified;
            ViewBag.Name = mdata.name;
            ViewBag.image = mdata.image;
            var shop = await connection.Shops_Table.Where(x => x.merchant_id == merchant_id).ToListAsync();
            var data = connection.Product_Table.Find(id);
            Add_Product_CombineModel add = new Add_Product_CombineModel();
            add.productdata = data;
            add.shopdata = shop;
            return View(add);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Add_Product_CombineModel data)
        {
            try
            {
                var oldproductdata = connection.Product_Table.Find(data.productdata.product_id);
                if (data.productdata.file != null)
                {
                    string folderpath = Path.Combine(env.WebRootPath, "images");

                    string extention = Path.GetExtension(data.productdata.file.FileName);
                    string newfilename = Guid.NewGuid().ToString();
                    newfilename = newfilename + extention;

                    string filepath = Path.Combine(folderpath, newfilename);
                    var file = new FileStream(filepath, FileMode.Create);
                    await data.productdata.file.CopyToAsync(file);

                    oldproductdata.product_image = newfilename;
                    oldproductdata.product_name = data.productdata.product_name;
                    oldproductdata.shop_id = data.productdata.shop_id;
                    oldproductdata.product_category = data.productdata.product_category;
                    oldproductdata.product_unit = data.productdata.product_unit;
                    oldproductdata.product_selling_price = data.productdata.product_selling_price;
                    oldproductdata.product_mrp = data.productdata.product_mrp;
                    oldproductdata.product_stock = data.productdata.product_stock;
                    oldproductdata.product_lowstock = data.productdata.product_lowstock;
                    oldproductdata.product_discription = data.productdata.product_discription;
                    connection.Product_Table.Update(oldproductdata);
                    connection.SaveChanges();

                    string oldfilepath = Path.Combine(folderpath, oldproductdata.product_image);

                    if (System.IO.File.Exists(oldfilepath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldfilepath);
                        }
                        catch (Exception ex)
                        {
                            //NoContent data found
                        }
                    }

                    TempData["msg"] = "Product Update Successfully";
                    return RedirectToAction("ProductAndStock","Merchant");
                }
                else
                {
                    oldproductdata.product_name = data.productdata.product_name;
                    oldproductdata.shop_id = data.productdata.shop_id;
                    oldproductdata.product_category = data.productdata.product_category;
                    oldproductdata.product_unit = data.productdata.product_unit;
                    oldproductdata.product_selling_price = data.productdata.product_selling_price;
                    oldproductdata.product_mrp = data.productdata.product_mrp;
                    oldproductdata.product_stock = data.productdata.product_stock;
                    oldproductdata.product_lowstock = data.productdata.product_lowstock;
                    oldproductdata.product_discription = data.productdata.product_discription;
                    connection.Product_Table.Update(oldproductdata);
                    connection.SaveChanges();

                    TempData["msg"] = "Product Update Successfully";
                    return RedirectToAction("ProductAndStock","Merchant");
                }
            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("EditProduct", "Merchant");
            }
        }

        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var data = connection.Product_Table.Find(id);
                string folderpath = Path.Combine(env.WebRootPath, "images");
                string filepath = Path.Combine(folderpath,data.product_image);
                
                connection.Product_Table.Remove(data);
                connection.SaveChanges() ;

                if (System.IO.File.Exists(filepath))
                {
                    try
                    {
                        System.IO.File.Delete(filepath);
                    }
                    catch (Exception ex)
                    {
                        //no data found
                    }
                }

                TempData["msg"] = "Product Delete Successfully";
                return RedirectToAction("ProductAndStock", "Merchant");

            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("ProductAndStock", "Merchant");
            }
        }

        public async Task<IActionResult> AddProduct()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            ViewBag.Name = data.name;
            ViewBag.image = data.image;
            ViewBag.isVerified = data.isVerified;
            if (data.isVerified != true)
            {
                TempData["nomsg"] = "You are not Verified Merchant";
                return RedirectToAction("Index", "Merchant");
            }
            var shop = await connection.Shops_Table.Where(x=>x.merchant_id == id).ToListAsync();
            Add_Product_CombineModel add = new Add_Product_CombineModel();
            add.shopdata = shop;
            if(shop.IsNullOrEmpty())
            {
                TempData["nomsg"] = "First Add Your Shop!. After you can add product in your shop.";
                return RedirectToAction("Index", "Merchant");
            }
            return View(add);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewProduct(Add_Product_CombineModel data)
        {
            try
            {
                if(data.productdata.file != null)
                {
                    int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
                    string extention = Path.GetExtension(data.productdata.file.FileName);
                    string newfilename = Guid.NewGuid().ToString();
                    newfilename = newfilename + extention;

                    string folderpath = Path.Combine(env.WebRootPath, "images");
                    string filepath = Path.Combine(folderpath, newfilename);
                    var file = new FileStream(filepath, FileMode.Create);
                    await data.productdata.file.CopyToAsync(file);

                    data.productdata.product_image = newfilename;
                    data.productdata.merchant_id = id;
                    connection.Product_Table.Add(data.productdata);
                    connection.SaveChanges();

                    TempData["msg"] = "Product Add Successfully";
                    return RedirectToAction("ProductAndStock");
                }
                else
                {
                    TempData["nomsg"] = "Please Upload Product Image!";
                    return RedirectToAction("AddProduct", "Merchant");
                }
            }catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("AddProduct", "Merchant");
            }
        }

        public async Task<IActionResult> MyShop()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            ViewBag.isVerified = data.isVerified;
            ViewBag.Name = data.name;
            ViewBag.image = data.image;
            var shop = await connection.Shops_Table.Where(x => x.merchant_id == id).ToListAsync();
            return View(shop);
        }

        public IActionResult ShopSetting(int id)
        {
            int mid = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var mdata = connection.MerchantTable.Find(mid);
            ViewBag.isVerified = mdata.isVerified;
            ViewBag.Name = mdata.name;
            ViewBag.image = mdata.image;
            var data = connection.Shops_Table.Find(id);
            return View(data);
        }

        public IActionResult OpenShop(int id)
        {
            try
            {
                var data = connection.Shops_Table.Find(id);
                data.isOpen = true;
                connection.Shops_Table.Update(data);
                connection.SaveChanges();
                TempData["msg"] = "Your Shop " + data.shopname + " Opened";
                return RedirectToAction("MyShop", "Merchant");
            }catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("MyShop", "Merchant");
            }
        }

        public IActionResult CloseShop(int id)
        {
            try
            {
                var data = connection.Shops_Table.Find(id);
                data.isOpen = false;
                connection.Shops_Table.Update(data);
                connection.SaveChanges();
                TempData["msg"] = "Your Shop " + data.shopname + " Closed";
                return RedirectToAction("MyShop", "Merchant");
            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("MyShop", "Merchant");
            }
        }

        public async Task<IActionResult> VerifyTheShop(int id)
        {
            try
            {
                var data = connection.Shops_Table.Find(id);
                Random random = new Random();
                int otp = random.Next(100000, 999999);

                string tosend = data.shop_email;
                string subject = "OTP For Shop Verification";
                string body = "Your otp is " + otp + " for verification of your shop do not share with any one.";
                await emailSender.SendEmail(tosend, subject, body);
                HttpContext.Session.SetInt32("verifyOTP",otp);
                HttpContext.Session.SetInt32("shopid",id);
                TempData["msg"] = "OTP sent to" + data.shop_email +"";
                return RedirectToAction("VerifyShop", "Merchant");
            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("MyShop", "Merchant");
            }
        }

        public IActionResult VerifyShop()
        {
            if (HttpContext.Session.GetInt32("verifyOTP")==null)
            {
                return RedirectToAction("MyShop", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyShopOTP(ForEmailAndOTP data)
        {
            int? otp = HttpContext.Session.GetInt32("verifyOTP");
            int? shopid = HttpContext.Session.GetInt32("shopid");
            if(data.otp == otp)
            {
                var shopdata = connection.Shops_Table.Find(shopid);
                shopdata.isVerified = true;
                connection.Shops_Table.Update(shopdata);
                connection.SaveChanges();
                HttpContext.Session.Remove("verifyOTP");
                HttpContext.Session.Remove("shopid");
                TempData["msg"] = ""+shopdata.shopname+" Verified Successfully";
                return RedirectToAction("MyShop", "Merchant");
            }
            else
            {
                TempData["nomsg"] = "Invalid OTP!";
                return RedirectToAction("VerifyShop", "Merchant");
            }
        }

        public IActionResult AddNewShop()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            ViewBag.isVerified = data.isVerified;
            ViewBag.Name = data.name;
            ViewBag.image = data.image;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddShop(Shops_Table shopdata)
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
                var data = connection.MerchantTable.Find(id);
                
                string suffix = "@gmail.com";
                if (shopdata.shop_email.Contains(suffix))
                {
                    if(shopdata.file != null)
                    {
                        string extention = Path.GetExtension(shopdata.file.FileName);
                        string filename = Guid.NewGuid().ToString();
                        filename = filename + extention;

                        string folderpath = Path.Combine(env.WebRootPath, "images");
                        string filepath = Path.Combine(folderpath, filename);
                        var file = new FileStream(filepath, FileMode.Create);
                        await shopdata.file.CopyToAsync(file);

                        shopdata.isVerified = false;
                        shopdata.isOpen = false;
                        shopdata.merchant_id = id;
                        shopdata.image = filename;
                        
                        connection.Shops_Table.Add(shopdata);
                        connection.SaveChanges();

                        string tosend = data.email;
                        string subject = "Your shop " + shopdata.shopname + " Listed Successfully";
                        string body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                        <meta charset='UTF-8'>
                        </head>

                        <body style='margin:0;padding:0;background:#f4f7fb;font-family:Arial,Helvetica,sans-serif;'>

                        <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f7fb;padding:30px 0;'>

                        <tr>
                        <td align='center'>

                        <table width='600' cellpadding='0' cellspacing='0'
                        style='background:#ffffff;border-radius:12px;overflow:hidden;
                        box-shadow:0 5px 20px rgba(0,0,0,.1);'>

                        <!-- Header -->

                        <tr>
                        <td align='center'
                        style='background:linear-gradient(90deg,#2563EB,#10B981);padding:35px;'>

                        <img src='https://tse3.mm.bing.net/th/id/OIP.EJS_xjAH4bdM0sexwkaM8QHaE8?r=0&pid=Api&h=220&P=0'
                        width='120'
                        alt='ApnaBazaar'/>

                        <h1 style='margin-top:20px;color:white;font-size:30px;'>
                        ApnaBazaar
                        </h1>

                        <p style='color:white;font-size:15px;margin-top:5px;'>
                        Connecting with People
                        </p>

                        </td>
                        </tr>

                        <!-- Body -->

                        <tr>
                        <td style='padding:40px;'>

                        <h2 style='margin:0;color:#222;'>
                        Welcome to ApnaBazaar! Your Shop {shopdata.shopname} Listed Successfully ❤️
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

                        Waiting for Verification after verification you can manage the shop.
                        We’re delighted to have you join our growing family! 🛍️ 
                        Thank you for choosing ApnaBazaar, your trusted destination 
                        for discovering local shops and shopping for your everyday needs. 
                        Our mission is to bring your favourite local stores closer to you, 
                        making shopping easier, faster, and more convenient. We’re excited 
                        to be a part of your journey and look forward to providing you with a 
                        wonderful shopping experience. 💖

                        </p>

                        <h2 style='margin:0;color:#222;'>
                        Happy Shopping! 🛒
                        </h2>

                        

                        <p style='color:#777;font-size:15px;'>

                       Warm regards,<br>
                       <b>Team ApnaBazaar</b><br>
                       Your Local Market, Just a Click Away! 💚

                        

                        </p>

                       

                        </td>
                        </tr>

                        <!-- Footer -->

                        <tr>

                        <td align='center'
                        style='background:#f7f7f7;padding:25px;'>

                        <p style='margin:0;font-size:14px;color:#888;'>

                        © 2026 ApnaBazaar

                        </p>

                        <p style='margin-top:8px;font-size:13px;color:#999;'>

                        Connecting with People

                        </p>

                        </td>

                        </tr>

                        </table>

                        </td>

                        </tr>

                        </table>

                        </body>

                        </html>";
                        await emailSender.SendEmail(tosend, subject, body);

                        TempData["msg"] = "Shop Register Successfully";
                        return RedirectToAction("MyShop", "Merchant");
                    }
                    else
                    {
                        shopdata.isVerified = false;
                        shopdata.isOpen = false;
                        shopdata.merchant_id = id;
                        connection.Shops_Table.Add(shopdata);
                        connection.SaveChanges();
                        string tosend = data.email;
                        string subject = "Your shop "+shopdata.shopname+" Listed Successfully";
                        string body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                        <meta charset='UTF-8'>
                        </head>

                        <body style='margin:0;padding:0;background:#f4f7fb;font-family:Arial,Helvetica,sans-serif;'>

                        <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f7fb;padding:30px 0;'>

                        <tr>
                        <td align='center'>

                        <table width='600' cellpadding='0' cellspacing='0'
                        style='background:#ffffff;border-radius:12px;overflow:hidden;
                        box-shadow:0 5px 20px rgba(0,0,0,.1);'>

                        <!-- Header -->

                        <tr>
                        <td align='center'
                        style='background:linear-gradient(90deg,#2563EB,#10B981);padding:35px;'>

                        <img src='https://tse3.mm.bing.net/th/id/OIP.EJS_xjAH4bdM0sexwkaM8QHaE8?r=0&pid=Api&h=220&P=0'
                        width='120'
                        alt='ApnaBazaar'/>

                        <h1 style='margin-top:20px;color:white;font-size:30px;'>
                        ApnaBazaar
                        </h1>

                        <p style='color:white;font-size:15px;margin-top:5px;'>
                        Connecting with People
                        </p>

                        </td>
                        </tr>

                        <!-- Body -->

                        <tr>
                        <td style='padding:40px;'>

                        <h2 style='margin:0;color:#222;'>
                        Welcome to ApnaBazaar! Your Shop {shopdata.shopname} Listed Successfully ❤️
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

                        Waiting for Verification after verification you can manage the shop.
                        We’re delighted to have you join our growing family! 🛍️ 
                        Thank you for choosing ApnaBazaar, your trusted destination 
                        for discovering local shops and shopping for your everyday needs. 
                        Our mission is to bring your favourite local stores closer to you, 
                        making shopping easier, faster, and more convenient. We’re excited 
                        to be a part of your journey and look forward to providing you with a 
                        wonderful shopping experience. 💖

                        </p>

                        <h2 style='margin:0;color:#222;'>
                        Happy Shopping! 🛒
                        </h2>

                        

                        <p style='color:#777;font-size:15px;'>

                       Warm regards,<br>
                       <b>Team ApnaBazaar</b><br>
                       Your Local Market, Just a Click Away! 💚

                        

                        </p>

                       

                        </td>
                        </tr>

                        <!-- Footer -->

                        <tr>

                        <td align='center'
                        style='background:#f7f7f7;padding:25px;'>

                        <p style='margin:0;font-size:14px;color:#888;'>

                        © 2026 ApnaBazaar

                        </p>

                        <p style='margin-top:8px;font-size:13px;color:#999;'>

                        Connecting with People

                        </p>

                        </td>

                        </tr>

                        </table>

                        </td>

                        </tr>

                        </table>

                        </body>

                        </html>";
                        await emailSender.SendEmail(tosend, subject, body);

                        TempData["msg"] = "Shop Register Successfully";
                        return RedirectToAction("MyShop", "Merchant");
                    }
                } 
                else
                {
                    TempData["nomsg"] = "Invalid Email Format!. Enter the email in valid format";
                    return RedirectToAction("AddNewShop", "Merchant");
                }

            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("AddNewShop", "Merchant");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateShop(Shops_Table shop)
        {
            try
            {
                var data = connection.Shops_Table.Find(shop.shop_id);

                    if (shop.file != null)
                    {
                        string folderpath = Path.Combine(env.WebRootPath, "images");
                        string oldfilepath = Path.Combine(folderpath, data.image);

                        if(data.image != null)
                        {
                            if (System.IO.File.Exists(oldfilepath))
                            {
                                try
                                {
                                    System.IO.File.Delete(oldfilepath);

                                }catch (Exception ex)
                                {
                                    //NoContent data found
                                }
                            }
                        }

                        string extention = Path.GetExtension(shop.file.FileName);
                        string filename = Guid.NewGuid().ToString();
                        filename = filename + extention;

                        string filepath = Path.Combine(folderpath, filename);
                        var file = new FileStream(filepath, FileMode.Create);
                        await shop.file.CopyToAsync(file);

                        data.shopname = shop.shopname;
                        data.shop_category = shop.shop_category;
                        data.shop_tagline = shop.shop_tagline;
                        data.shop_manager_name = shop.shop_manager_name;
                        data.mobile = shop.mobile;
                        data.whatsapp_number = shop.whatsapp_number;
                        data.upi_id = shop.upi_id;
                        data.shop_gstin_number = shop.shop_gstin_number;
                        data.shop_full_address = shop.shop_full_address;
                        data.shop_landmark = shop.shop_landmark;
                        data.shop_pincode = shop.shop_pincode;
                        data.shop_city = shop.shop_city;
                        data.home_delivery_radius = shop.home_delivery_radius;
                        data.shop_opening_time = shop.shop_opening_time;
                        data.shop_closing_time = shop.shop_closing_time;
                        data.shop_latitude = shop.shop_latitude;
                        data.shop_longitude = shop.shop_longitude;
                        data.image = filename;
                        
                        connection.Shops_Table.Update(data);
                        connection.SaveChanges();

                        TempData["msg"] = "Shop Update Successfully";
                        return RedirectToAction("MyShop", "Merchant");
                    }
                    else
                    {

                        data.shopname = shop.shopname;
                        data.shop_category = shop.shop_category;
                        data.shop_tagline = shop.shop_tagline;
                        data.shop_manager_name = shop.shop_manager_name;
                        data.mobile = shop.mobile;
                        data.whatsapp_number = shop.whatsapp_number;
                        data.upi_id = shop.upi_id;
                        data.shop_gstin_number = shop.shop_gstin_number;
                        data.shop_full_address = shop.shop_full_address;
                        data.shop_landmark = shop.shop_landmark;
                        data.shop_pincode = shop.shop_pincode;
                        data.shop_city = shop.shop_city;
                        data.home_delivery_radius = shop.home_delivery_radius;
                        data.shop_opening_time = shop.shop_opening_time;
                        data.shop_closing_time = shop.shop_closing_time;
                        data.shop_latitude = shop.shop_latitude;
                        data.shop_longitude = shop.shop_longitude;

                        connection.Shops_Table.Update(data);
                        connection.SaveChanges();

                        TempData["msg"] = "Shop Update Successfully";
                        return RedirectToAction("MyShop", "Merchant");
                    }
                
              

            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("AddNewShop", "Merchant");
            }
        } 

        public IActionResult MerchantLogout()
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
                var data = connection.MerchantTable.Find(id);
                HttpContext.Session.Clear();
                TempData["msg"] = "" + data.name + " logged out!";
                return RedirectToAction("MerchantLogin", "Login");
            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something went wrong!";
                return RedirectToAction("Index", "Merchant");
            }
        }

        public IActionResult MerchantProfile()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            ViewBag.Name = data.name;
            ViewBag.image = data.image;
            ViewBag.isVerified = data.isVerified;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMerchant(MerchantTable merchat)
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
                var data = connection.MerchantTable.Find(id);

                if (merchat.file != null)
                {
                    var folderpath = Path.Combine(env.WebRootPath, "images");

                    if (data.image != null)
                    {
                        var oldfilepath = Path.Combine(folderpath, data.image);
                        if (System.IO.File.Exists(oldfilepath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldfilepath);

                            }
                            catch (Exception ex)
                            {
                                // data not found
                            }
                        }
                    }


                    var extention = Path.GetExtension(merchat.file.FileName);
                    var newfilename = Guid.NewGuid().ToString();
                    newfilename = newfilename + extention;

                    var filepath = Path.Combine(folderpath, newfilename);
                    var file = new FileStream(filepath, FileMode.Create);
                    await merchat.file.CopyToAsync(file);

                    data.name = merchat.name;
                    data.mobile = merchat.mobile;
                    data.altMobile = merchat.altMobile;
                    data.address = merchat.address;
                    data.dob = merchat.dob;
                    data.gender = merchat.gender;
                    data.image = newfilename;
                    connection.MerchantTable.Update(data);
                    connection.SaveChanges();
                    TempData["msg"] = "Profile Update Successfully";
                    return RedirectToAction("MerchantProfile", "Merchant");
                }
                else
                {

                    data.name = merchat.name;
                    data.mobile = merchat.mobile;
                    data.altMobile = merchat.altMobile;
                    data.address = merchat.address;
                    data.dob = merchat.dob;
                    data.gender = merchat.gender;
                    connection.MerchantTable.Update(data);
                    connection.SaveChanges();
                    TempData["msg"] = "Profile Update Successfully";
                    return RedirectToAction("MerchantProfile", "Merchant");
                }
            }
            catch (Exception ex)
            {
                TempData["nomsg"] = "Something went worng!.";
                return RedirectToAction("MerchantProfile", "Merchant");
            }
        }

        public IActionResult MerchantChangePassword()
        {
            int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
            var data = connection.MerchantTable.Find(id);
            ViewBag.Name = data.name;
            ViewBag.image = data.image;
            ViewBag.isVerified = data.isVerified;
            return View();
        }

        [HttpPost]
        public IActionResult MerchantUpdatePassword(changepassword merchant)
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("merchant_id"));
                var data = connection.MerchantTable.Find(id);

                if (data.password == merchant.oldpassword)
                {
                    if (merchant.newpassword == merchant.confirmpassword)
                    {
                        data.password = merchant.newpassword;
                        connection.MerchantTable.Update(data);
                        connection.SaveChanges();
                        TempData["msg"] = "Password Change Successfully";
                        return RedirectToAction("MerchantChangePassword", "Merchant");
                    }
                    else
                    {
                        TempData["nomsg"] = "Confirm Password Not Match!";
                        return RedirectToAction("MerchantChangePassword", "Merchant");
                    }
                }
                else
                {
                    TempData["nomsg"] = "Old Password is Wrong!. Please enter right password";
                    return RedirectToAction("MerchantChangePassword", "Merchant");
                }
            }catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong!";
                return RedirectToAction("MerchantChangePassword", "Merchant");
            }
        }

    }
}

