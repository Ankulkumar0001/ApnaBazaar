using ApnaBazaar.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApnaBazaar.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UserController : Controller
    {
        DatabaseConnection connection;
        IWebHostEnvironment env;
        public UserController(DatabaseConnection connection,IWebHostEnvironment env) 
        {
            this.connection = connection;
            this.env = env;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (HttpContext.Session.GetString("userid") == null)
            {
                context.Result = RedirectToAction("Index", "Login");
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Shops()
        {
            return View();
        }
        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Profile()
        {
            int id = int.Parse(HttpContext.Session.GetString("userid"));
            var data = connection.UserTable.Find(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserTable user)
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("userid"));
                var data = connection.UserTable.Find(id);

                if (user.file != null)
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


                    var extention = Path.GetExtension(user.file.FileName);
                    var newfilename = Guid.NewGuid().ToString();
                    newfilename = newfilename + extention;

                    var filepath = Path.Combine(folderpath, newfilename);
                    var file = new FileStream(filepath, FileMode.Create);
                    await user.file.CopyToAsync(file);

                    data.name = user.name;
                    data.image = newfilename;
                    data.mobile = user.mobile;
                    data.gender = user.gender;
                    connection.UserTable.Update(data);
                    connection.SaveChanges();
                    HttpContext.Session.SetString("image", newfilename);
                    TempData["msg"] = "Profile Update Successfully";
                    return RedirectToAction("Profile", "User");
                }
                else
                {

                    data.name = user.name;
                    data.mobile = user.mobile;
                    data.gender = user.gender;
                    connection.UserTable.Update(data);
                    connection.SaveChanges();
                    TempData["msg"] = "Profile Update Successfully";
                    return RedirectToAction("Profile", "User");
                }
            }catch (Exception ex)
            {
                TempData["nomsg"] = "Something went worng!.";
                return RedirectToAction("Profile", "User");
            }
        }

        public IActionResult MyOrder()
        {
            return View();
        }

        public IActionResult MyWishlist()
        {
            return View();
        }

        public IActionResult MyAddresses()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangeUserpassword(changepassword data)
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("userid"));
                var user = connection.UserTable.Find(id);
                if (data.oldpassword == user.password)
                {
                    if (data.newpassword == data.confirmpassword)
                    {
                        user.password = data.newpassword;
                        connection.UserTable.Update(user);
                        connection.SaveChanges();
                        TempData["msg"] = "Password changed successfully";
                        return RedirectToAction("ChangePassword", "User");
                    }
                    else
                    {
                        TempData["nomsg"] = "New Password and Confirm Password not match.";
                        return RedirectToAction("ChangePassword", "User");
                    }
                }
                else
                {
                    TempData["nomsg"] = "Old password is wrong.";
                    return RedirectToAction("ChangePassword", "User");
                }
            }catch (Exception ex)
            {
                TempData["nomsg"] = "Something Went Wrong.";
                return RedirectToAction("ChangePassword", "User");
            }
        }

        public IActionResult Help()
        {
            return View();
        }

        public IActionResult Logout()
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("userid"));
                var data = connection.UserTable.Find(id);
                HttpContext.Session.Clear();
                TempData["msg"] = "" + data.name + " logged out!";
                return RedirectToAction("Index", "Login");
            }catch (Exception ex)
            {
                TempData["nomsg"] = "Something went wrong!";
                return RedirectToAction("Index", "User");
            }
        }
    }
}
