using ApnaBazaar.Models;
using ApnaBazaar.Services;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.WebRequestMethods;

namespace ApnaBazaar.Controllers
{
    public class LoginController : Controller
    {
        DatabaseConnection connection;
        EmailSender emailSender;
        public LoginController(DatabaseConnection connection,EmailSender emailSender) {
        
            this.connection = connection;
            this.emailSender = emailSender;
        }
        public IActionResult Index()
        {
            if(HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        public IActionResult UserRegister()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserTable data)
        {
            var user = connection.UserTable.FirstOrDefault(x => x.email == data.email);
            if (user != null)
            {
                TempData["nomsg"] = "Email already exist. Try with new email id";
                return RedirectToAction("UserRegister", "Login");
            }
            else
            {
                string valid = "@gmail.com";
                if (data.email.Contains(valid))
                {
                    if (data.password == data.confirmPassword)
                    {
                        connection.UserTable.Add(data);
                        connection.SaveChanges();
                        string tosent = data.email;
                        string subject = "You are successfully registered in ApnaBazaar - A Local Shopping Platform";
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
                        alt='SkillConnect Logo'/>

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
                        Welcome to ApnaBazaar! ❤️
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

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

                        await emailSender.SendEmail(tosent, subject, body);
                        TempData["msg"] = "User Register Successfully";
                        return RedirectToAction("Index", "Login");
                    }
                    else
                    {
                        TempData["nomsg"] = "Password Not match";
                        return RedirectToAction("UserRegister", "Login");
                    }
                }
                else
                {
                    TempData["nomsg"] = "Please enter the email in right format!";
                    return RedirectToAction("UserRegister", "Login");
                }
            }
        }

        [HttpPost]
        public IActionResult UsersLogin(UserLogin user)
        {
            
            var data = connection.UserTable.FirstOrDefault(x => x.email == user.email && x.password == user.password);
            if (data != null)
            {
                HttpContext.Session.SetString("userid", data.id.ToString());
                if(data.image != null)
                {
                    HttpContext.Session.SetString("image", data.image);
                }
                TempData["msg"] = ""+data.name+" Login Successfully";
                return RedirectToAction("Index", "User");
            }
            else
            {
                TempData["nomsg"] = "Wrong Cradential. Please Enter Valid login Cradential!";
                return RedirectToAction("Index", "Login");
            }
        }

        public IActionResult UserForgotPassword()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendForgotOTP(ForEmailAndOTP user)
        {
            var data = connection.UserTable.FirstOrDefault(x=>x.email == user.email);
            if(data != null)
            {
                Random random = new Random();
                int otp = random.Next(100000,999999);

                string toemail = user.email;
                string subject = "Password reset OTP";
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
                        alt='SkillConnect Logo'/>

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
                        Hello 👋
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

                       
                        <b>ApnaBazaar</b>.

                        Use the OTP code below to Verify your email address.

                        </p>

                        <div style='
                        margin:35px auto;
                        width:230px;
                        padding:20px;
                        background:#2563EB;
                        border-radius:10px;
                        text-align:center;
                        '>

                        <span style='
                        font-size:36px;
                        font-weight:bold;
                        letter-spacing:10px;
                        color:white;'>

                        {otp}

                        </span>

                        </div>

                        <p style='color:#777;font-size:15px;'>

                        This OTP is valid for

                        <b>5 Minutes</b>

                        </p>

                        <p style='color:#777;font-size:15px;'>

                        Please don't share this OTP with anyone.

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

                await emailSender.SendEmail(toemail, subject, body);
                HttpContext.Session.SetString("otp",otp.ToString());
                HttpContext.Session.SetString("email", data.email);

                TempData["msg"] = "OTP sent to "+user.email+"";
                return RedirectToAction("UserVerifyOTP", "Login");
            }
            else
            {
                TempData["nomsg"] = "Please Enter Registered Email ID!";
                return RedirectToAction("UserForgotPassword", "Login");
            }
        }

        public IActionResult UserVerifyOTP()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOTP(ForEmailAndOTP user)
        {
            int oldotp = int.Parse(HttpContext.Session.GetString("otp"));
            int otp = user.otp;
            if(oldotp == otp)
            {
                TempData["msg"] = "OTP Verified. You can reset your password";
                return RedirectToAction("UserResetPassword", "Login");
            }
            else
            {
                TempData["nomsg"] = "OTP Invalid. Please try again!";
                return RedirectToAction("UserVerifyOTP", "Login");
            }
        }

        public IActionResult UserResetPassword()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(UserTable user)
        {
            string email = HttpContext.Session.GetString("email");
            var data = connection.UserTable.FirstOrDefault(x=>x.email == email);
            if(user.password == user.confirmPassword)
            {
                data.password = user.password;
                connection.UserTable.Update(data);
                connection.SaveChanges();
                TempData["msg"] = "Password reset successfully";
                return RedirectToAction("Index", "Login");
            }
            else
            {
                TempData["nomsg"] = "Password not match!";
                return RedirectToAction("UserResetPassword", "Login");
            }
        }

        public IActionResult LoginWithOTP()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginOTP(ForEmailAndOTP user)
        {
            var data = connection.UserTable.FirstOrDefault(x=>x.email == user.email);
            if (data != null)
            {
                Random random = new Random();
                int otp = random.Next(100000, 999999);

                string toemail = user.email;
                string subject = "OTP For Login";
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
                        alt='SkillConnect Logo'/>

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
                        Hello 👋
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

                       
                        <b>ApnaBazaar</b>.

                        Use the OTP code below to login your account.

                        </p>

                        <div style='
                        margin:35px auto;
                        width:230px;
                        padding:20px;
                        background:#2563EB;
                        border-radius:10px;
                        text-align:center;
                        '>

                        <span style='
                        font-size:36px;
                        font-weight:bold;
                        letter-spacing:10px;
                        color:white;'>

                        {otp}

                        </span>

                        </div>

                        <p style='color:#777;font-size:15px;'>

                        This OTP is valid for

                        <b>5 Minutes</b>

                        </p>

                        <p style='color:#777;font-size:15px;'>

                        Please don't share this OTP with anyone.

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

                await emailSender.SendEmail(toemail, subject, body);
                HttpContext.Session.SetString("otp", otp.ToString());
                HttpContext.Session.SetString("email", data.email);

                TempData["msg"] = "OTP sent to " + user.email + "";
                return RedirectToAction("VeriyLOTP", "Login");

            }
            else
            {
                TempData["nomsg"] = "Please Enter Registered Email ID!";
                return RedirectToAction("LoginWithOTP", "Login");
            }
        }

        public IActionResult VeriyLOTP()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyLoginOTP(ForEmailAndOTP user)
        {
            int otp = int.Parse(HttpContext.Session.GetString("otp"));
            int newotp = user.otp;
            
            if(otp == newotp)
            {
                string email = HttpContext.Session.GetString("email");
                var data = connection.UserTable.FirstOrDefault(x => x.email == email);
                HttpContext.Session.SetString("userid", data.id.ToString());
                TempData["msg"] = ""+data.name+" login successfully";
                return RedirectToAction("Index", "User");
            }
            else
            {
                TempData["nomsg"] = "OTP Invalid!.Please enter valid opt";
                return RedirectToAction("VeriyLOTP", "Login");
            }
        }


        //Merchant  Controller Auth start

        public IActionResult MerchantLogin()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public IActionResult LoginMerchant(UserLogin user)
        {
            var data = connection.MerchantTable.FirstOrDefault(x => x.email == user.email && x.password == user.password);
            if(data != null)
            {
                HttpContext.Session.SetString("merchant_id", data.id.ToString());
                TempData["msg"] = "" + data.name + " Login Successfully";
                return RedirectToAction("Index", "Merchant");
                
            }
            else
            {
                TempData["nomsg"] = "Wrong Cradential. Please Enter Valid login Cradential!";
                return RedirectToAction("MerchantLogin", "Login");
            }
        }

        public IActionResult MerchantRegister()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMerchant(MerchantTable merchant)
        {
            var data = connection.MerchantTable.FirstOrDefault(x=>x.email == merchant.email);
            if(data == null)
            {
               string valid = "@gmail.com";
               if(merchant.email.Contains(valid))
                {
                    if (merchant.aadharNumber.Length == 12)
                    {
                        if (merchant.password == merchant.confirmPassword)
                        {
                            merchant.isVerified = false;
                            connection.MerchantTable.Add(merchant);
                            connection.SaveChanges();

                            string toemail = merchant.email;
                            string subject = "You are successfully registered as Merchant in ApnaBazaar - A Local Shopping Platform";
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
                        Welcome to ApnaBazaar! ❤️
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

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
                            await emailSender.SendEmail(toemail, subject, body);

                            TempData["msg"] = "" + merchant.name + " Register Successfully";
                            return RedirectToAction("MerchantLogin", "Login");
                        }
                        else
                        {
                            TempData["nomsg"] = "Password not match";
                            return RedirectToAction("MerchantRegister", "Login");
                        }
                       
                    }
                    else
                    {
                        TempData["nomsg"] = "Aadhar Number Must be contains 12 digits.";
                        return RedirectToAction("MerchantRegister", "Login");
                    }
                }
                else
                {
                    TempData["nomsg"] = "Please enter the email in right format!";
                    return RedirectToAction("MerchantRegister", "Login");
                }
            }
            else
            {
                TempData["nomsg"] = "Email id already registered. Please try with another email id.";
                return RedirectToAction("MerchantRegister", "Login");
            }
        }

        public IActionResult MerchantForgotPassword()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotMerchantPass(ForEmailAndOTP user)
        {
            var data = connection.MerchantTable.FirstOrDefault(x => x.email == user.email);
            if(data != null)
            {
                Random randon = new Random();
                int otp = randon.Next(100000, 999999);

                string tosend = user.email;
                string subject = "Password reset OTP";
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
                        alt='SkillConnect Logo'/>

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
                        Hello 👋
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

                       
                        <b>ApnaBazaar</b>.

                        Use the OTP code below to Verify your email address.

                        </p>

                        <div style='
                        margin:35px auto;
                        width:230px;
                        padding:20px;
                        background:#2563EB;
                        border-radius:10px;
                        text-align:center;
                        '>

                        <span style='
                        font-size:36px;
                        font-weight:bold;
                        letter-spacing:10px;
                        color:white;'>

                        {otp}

                        </span>

                        </div>

                        <p style='color:#777;font-size:15px;'>

                        This OTP is valid for

                        <b>5 Minutes</b>

                        </p>

                        <p style='color:#777;font-size:15px;'>

                        Please don't share this OTP with anyone.

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

                HttpContext.Session.SetString("otp", otp.ToString());
                HttpContext.Session.SetString("email", user.email);

                TempData["msg"] = "OTP sent to "+user.email+"";
                return RedirectToAction("MerchantVerifyOTP", "Login");

            }
            else
            {
                TempData["nomsg"] = "Please Enter Registered Email ID";
                return RedirectToAction("MerchantForgotPassword", "Login");
            }
        }

        public IActionResult MerchantVerifyOTP()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyMOTP(ForEmailAndOTP user)
        {
            int oldotp = int.Parse(HttpContext.Session.GetString("otp"));

            if(oldotp == user.otp)
            {
                TempData["msg"] = "OTP Verified Successfully. You can reset your password";
                return RedirectToAction("MerchantResetPassword", "Login");
            }
            else
            {
                TempData["nomsg"] = "Invalid OTP. Please enter valid otp.";
                return RedirectToAction("MerchantVerifyOTP", "Login");
            }
        }

        public IActionResult MerchantResetPassword()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ResetMerchantPass(MerchantTable user)
        {
            string email = HttpContext.Session.GetString("email");
            var data = connection.MerchantTable.FirstOrDefault(x => x.email == email);
            if(user.password == user.confirmPassword)
            {
                data.password = user.password;
                connection.MerchantTable.Update(data);
                connection.SaveChanges();
                TempData["msg"] = "Password Reset Successfully. Now try to login";
                return RedirectToAction("MerchantLogin", "Login");
            }
            else
            {
                TempData["nomsg"] = "Password not match!";
                return RedirectToAction("MerchantResetPassword", "Login");
            }
        }

        public IActionResult MerchantLoginWithOTP()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MerchantOTPLogin(ForEmailAndOTP user)
        {
            var data = connection.MerchantTable.FirstOrDefault(y => y.email == user.email);
            if(data != null)
            {
                Random random = new Random();
                int otp = random.Next(100000, 999999);

                string toemail = user.email;
                string subject = "OTP for merchant login";
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
                        alt='SkillConnect Logo'/>

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
                        Hello 👋
                        </h2>

                        <p style='font-size:16px;color:#555;line-height:28px;'>

                       
                        <b>ApnaBazaar</b>.

                        Use the OTP code below to login your account.

                        </p>

                        <div style='
                        margin:35px auto;
                        width:230px;
                        padding:20px;
                        background:#2563EB;
                        border-radius:10px;
                        text-align:center;
                        '>

                        <span style='
                        font-size:36px;
                        font-weight:bold;
                        letter-spacing:10px;
                        color:white;'>

                        {otp}

                        </span>

                        </div>

                        <p style='color:#777;font-size:15px;'>

                        This OTP is valid for

                        <b>5 Minutes</b>

                        </p>

                        <p style='color:#777;font-size:15px;'>

                        Please don't share this OTP with anyone.

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
                await emailSender.SendEmail(toemail, subject, body);

                HttpContext.Session.SetString("otp",otp.ToString());
                HttpContext.Session.SetString("email",user.email);

                TempData["msg"] = "OTP sent to "+user.email+"";
                return RedirectToAction("VerifyMerchantLoginOTP", "Login");
            }
            else
            {
                TempData["nomsg"] = "Please Enter Registered Email ID!";
                return RedirectToAction("MerchantLoginWithOTP", "Login");
            }
        }

        public IActionResult VerifyMerchantLoginOTP()
        {
            if (HttpContext.Session.GetString("merchant_id") != null)
            {
                return RedirectToAction("Index", "Merchant");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyMLOTP(ForEmailAndOTP user)
        {
            int oldotp = int.Parse(HttpContext.Session.GetString("otp"));
            string email = HttpContext.Session.GetString("email");

            if(oldotp == user.otp)
            {
                var data = connection.MerchantTable.FirstOrDefault(x => x.email == email);
                HttpContext.Session.SetString("merchant_id", data.id.ToString());
                TempData["msg"] = ""+data.name+" login successfully";
                return RedirectToAction("Index", "Merchant");
            }
            else
            {
                TempData["nomsg"] = "Invalid OTP. Please enter valid OTP";
                return RedirectToAction("VerifyMerchantLoginOTP", "Login");
            }
        }


    }
}
