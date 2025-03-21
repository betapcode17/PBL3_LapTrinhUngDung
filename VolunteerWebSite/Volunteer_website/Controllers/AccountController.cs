using Microsoft.AspNetCore.Mvc;
using Volunteer_website.ViewModel;
using Volunteer_website.Models;
using Volunteer_website.Data;
using AutoMapper;
using Volunteer_website.Helpers;
using MyCommerce.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Volunteer_website.Controllers
{
    public class AccountController : Controller
    {
        private readonly VolunteerManagementContext db;
        private readonly IMapper _mapper;

        public AccountController(VolunteerManagementContext context, IMapper mapper)
        {
            this.db = context;
            _mapper = mapper;
        }

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterVM();
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
              

                try
                {
                    // Map RegisterVM sang User
                    var user = _mapper.Map<User>(model);
                    user.UserId = Guid.NewGuid().ToString(); // Sinh UserId tự động
                    user.RandomKey = Util.GenerateRandomkey();
                    user.UserName = model.UserName;
                    user.Password = model.Password.ToMd5Hash(user.RandomKey);
                    user.Role = 0;
                    user.is_active = true;
                    // Map RegisterVM sang Volunteer
                    var volunteer = _mapper.Map<Volunteer>(model);
                    volunteer.VolunteerId = user.UserId; // Gán UserId cho VolunteerId

                    // Thêm vào database
                    db.Users.Add(user);
                    db.Volunteers.Add(volunteer);
                    db.SaveChanges(); // Lưu vào DB

                    TempData["SuccessMessage"] = "Đăng ký thành công!";
                    return RedirectToAction("Index", "Home"); // Chuyển hướng sau khi đăng ký thành công
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi đăng ký: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ!");
            }
            return View(model);
        }
        #endregion

        #region Login
        [HttpGet]
        public ActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginVM model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                ModelState.Values.SelectMany(v => v.Errors).ToList().ForEach(error =>
                    Console.WriteLine("Lỗi ModelState: " + error.ErrorMessage));
                return View(model);
            }

            var user = db.Users.SingleOrDefault(kh => kh.UserName == model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("loi", "Không tìm thấy người dùng.");
                return View(model);
            }

            if (!user.is_active)
            {
                ModelState.AddModelError("loi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
                return View(model);
            }

            if (user.Password != model.Password.ToMd5Hash(user.RandomKey))
            {
                ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
                return View(model);
            }

            var volunteer = db.Volunteers.SingleOrDefault(vol => vol.VolunteerId == user.UserId);
            if (volunteer == null)
            {
                ModelState.AddModelError("loi", "Thông tin tình nguyện viên không tồn tại.");
                return View(model);
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, volunteer.Name ?? ""),
            new Claim(ClaimTypes.Email, volunteer.Email ?? ""),
            new Claim(ClaimTypes.MobilePhone, volunteer.PhoneNumber ?? ""),
            new Claim(ClaimTypes.StreetAddress, volunteer.Address ?? ""),
            new Claim(ClaimTypes.Gender, volunteer.Gender ? "Male" : "Female"),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            try
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                Console.WriteLine("Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SignInAsync: " + ex.Message);
                return View(model);
            }

            return (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }


        #endregion
        [Authorize]
        public IActionResult profile()
        {

            return View();
        }
        [Authorize]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }

    }
}
