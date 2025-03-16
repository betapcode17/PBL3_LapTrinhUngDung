using Microsoft.AspNetCore.Mvc;
using Volunteer_website.ViewModel;
using Volunteer_website.Models;
using Volunteer_website.Data;
using AutoMapper;
using Volunteer_website.Helpers;
using MyCommerce.Models;

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
                // Kiểm tra xem User đã tồn tại chưa
                //var existingUser = db.Users.FirstOrDefault(u => u.UserName == model.UserName);
                //if (existingUser != null)
                //{
                //    ModelState.AddModelError("", "UserName đã tồn tại!");
                //    return View(model);
                //}

                try
                {
                    // Map RegisterVM sang User
                    var user = _mapper.Map<User>(model);
                    user.UserId = Guid.NewGuid().ToString(); // Sinh UserId tự động
                    user.RandomKey = Util.GenerateRandomkey();
                    user.Password = model.Password.ToMd5Hash(user.RandomKey);
                    user.Role = 0;

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

        public ActionResult Login()
        {
            return View();
        }
    }
}
