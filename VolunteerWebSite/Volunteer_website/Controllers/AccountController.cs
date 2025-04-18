using Microsoft.AspNetCore.Mvc;
using Volunteer_website.ViewModel;
using Volunteer_website.Models;
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
                // Kiểm tra username đã tồn tại hay chưa
                if (db.Users.Any(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác!");
                    return View(model);
                }

                try
                {
                    var user = _mapper.Map<User>(model);
                    user.UserId = Guid.NewGuid().ToString();
                    user.RandomKey = Util.GenerateRandomkey();
                    user.UserName = model.UserName;
                    user.Password = model.Password.ToMd5Hash(user.RandomKey);
                    user.Role = 0;
                    user.IsActive = true;

                    var volunteer = _mapper.Map<Volunteer>(model);
                    volunteer.VolunteerId = user.UserId;

                    db.Users.Add(user);
                    db.Volunteers.Add(volunteer);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Đăng ký thành công!";
                    return RedirectToAction("Index", "Home");
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

            if (!user.IsActive)
            {
                ModelState.AddModelError("loi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
                return View(model);
            }

            if (user.Password != model.Password.ToMd5Hash(user.RandomKey))
            {
                ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
                return View(model);
            }

            if (user.Role == 0)
            {
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
                    new Claim(ClaimTypes.Gender, volunteer.Gender == true? "Male" : "Female"),
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

                // Role 0: Điều hướng về Index của Home Controller
                return (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    ? Redirect(returnUrl)
                    : RedirectToAction("Index", "Home");
            }
            else if (user.Role == 1)
            {
                var org = db.Organizations.SingleOrDefault(vol => vol.OrgId == user.UserId);
                if (org == null)
                {
                    ModelState.AddModelError("loi", "Thông tin tổ chức không tồn tại.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, org.Name ?? ""),
                    new Claim(ClaimTypes.Email, org.Email ?? ""),
                    new Claim(ClaimTypes.MobilePhone, org.PhoneNumber ?? ""),
                    new Claim(ClaimTypes.StreetAddress, org.Address ?? ""),
                    new Claim(ClaimTypes.Gender, org.Description ?? ""),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
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

                return (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    ? Redirect(returnUrl)
                    : Redirect(Url.Action("Index", "HomeOrg", new { area = "Organization" }));
            }
            else if(user.Role == 2)
            {
                var Admin = db.Admins.SingleOrDefault(vol => vol.AdminId == user.UserId);
                if (Admin == null)
                {
                    ModelState.AddModelError("loi", "Thông tin Admin không tồn tại.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Admin.Name ?? ""),
                    new Claim(ClaimTypes.Email, Admin.Email ?? ""),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
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

                return (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    ? Redirect(returnUrl)
                    : Redirect(Url.Action("Index", "HomeAdmin", new { area = "Admin" }));
            }


            // Nếu không thuộc role 0 hoặc 1, có thể thêm điều hướng mặc định hoặc thông báo lỗi
            ModelState.AddModelError("loi", "Vai trò không hợp lệ.");
            return View(model);
        }


        #endregion
        [Authorize]
        public IActionResult profile()
        {

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }

    }
}
