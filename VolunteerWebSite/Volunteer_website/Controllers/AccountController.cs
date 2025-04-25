using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using System.Web.Helpers;
using Volunteer_website.Data;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YourAppName.Controllers
{
    public class AccountController : Controller
    {
        private readonly VolunteerDbContext _context;

        public AccountController(VolunteerDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Update_Infor()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact_Infor()
        {       
            return View();
        }


        [HttpPost]
        public IActionResult Contact_Infor(Update_ContactModel updatedVolunteer)
        {
            var addressErrors = ModelState["Address"]?.Errors;
            var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in allErrors)
            {
                Console.WriteLine("Lỗi: " + error.ErrorMessage);
            }

            foreach (var error in addressErrors)
            {
                Console.WriteLine("Lỗi Address: " + error.ErrorMessage);
            }

            if (ModelState.IsValid)
            {
                var VolunteerId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(VolunteerId))
                {
                    return RedirectToAction("SignIn", "Account");
                }

                try
                {
                    var volunteer = _context.Volunteers.FirstOrDefault(v => v.VolunteerId == VolunteerId);
                    if (volunteer == null)
                    {
                        return NotFound();
                    }

                    volunteer.Name = updatedVolunteer.Name;
                    volunteer.Email = updatedVolunteer.Email;
                    volunteer.PhoneNumber = updatedVolunteer.PhoneNumber;
                    volunteer.Address = updatedVolunteer.Address;

                    _context.Volunteers.Update(volunteer);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi khi cập nhật thông tin: " + ex.Message);
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật.";
                }

                return RedirectToAction("Manage", new { id = VolunteerId });
            }

            return View(updatedVolunteer);
        }

        private string GetLoggedInUserId()
        {
            return HttpContext.Session.GetString("VolunteerId");
        }

        public IActionResult Change_Password()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Sign_Up()
        {
            return View();
        }

        // POST: /Account/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sign_Up(SignUpModel su)

        {
            if (ModelState.IsValid)
            {
                
                var existingUser = _context.Volunteers.FirstOrDefault(u => u.Email == su.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View("Sign_Up");
                }

                
                var existingAccount = _context.Accounts.FirstOrDefault(a => a.UserName == su.UserName);
                if (existingAccount != null)
                {
                    ModelState.AddModelError("UserName", "Tên người dùng đã tồn tại");
                    return View("SignUp");
                }
                string gen_id = Guid.NewGuid().ToString(); 

             
                var account = new Account
                {
                    UserId = gen_id, 
                    UserName = su.UserName,
                    Password = Crypto.HashPassword(su.Password) ,
                  
                };

                try
                {
                    _context.Accounts.Add(account);
                    _context.SaveChanges();
                    Console.WriteLine("Account ID: " + account.UserId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi khi lưu Account: " + ex.Message);
                }
                var volunteer = new Volunteer
                {
                    VolunteerId = gen_id, 
                    PhoneNumber = su.PhoneNumber,
                    Email = su.Email,
                    Name = su.Name,
                    DateOfBirth = su.DateOfBirth.HasValue ? DateOnly.FromDateTime(su.DateOfBirth.Value) : (DateOnly?)null,
                    Gender = su.Gender,
                    Address = string.Empty, // Giá trị mặc định
                    ImagePath = string.Empty, // Giá trị mặc định
                };
            

                try
                {
                    _context.Volunteers.Add(volunteer);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Lỗi khi lưu Volunteer: " + e.Message);
                }
                var user = new User
                {
                    UserId = account.UserId,
                    Role = "volunteer"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("SignIn", "Account");
            }
            else
            {

                 ModelState.AddModelError("", "Dữ liệu không hợp lệ");
                 return View(su);
            }           
        }
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(SignInModel model)
        {

            if (ModelState.IsValid)
            {
                
                var account = _context.Accounts
                    .FirstOrDefault(a => a.UserName == model.UserName);

                if (account != null && Crypto.VerifyHashedPassword(account.Password, model.Password))
                {
                

                    var user = _context.Users.FirstOrDefault(u => u.UserId == account.UserId);
                    string role = user?.Role ?? "volunteer"; // nếu không có thì mặc định luôn volunteer

                    HttpContext.Session.SetString("UserId", account.UserId.ToString());
                    HttpContext.Session.SetString("UserRole", role);
                    return RedirectToAction("IndexUser", "Home");
                }
             
                
                    ModelState.AddModelError("", "Invalid username or password.");               
            }

            return View(model);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        private bool IsUserInRole(string role)
        {
            var currentRole = HttpContext.Session.GetString("UserRole");
            return currentRole != null && currentRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }
        public IActionResult Manage()
        {
            return View();
        }
       
    }
}