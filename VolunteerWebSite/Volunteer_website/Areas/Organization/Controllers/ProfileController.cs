using Microsoft.AspNetCore.Mvc;
using Volunteer_website.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volunteer_website.Helpers; // Thêm namespace của UpLoadImgService
using MyCommerce.Models; // Thêm namespace của DataEncryptionExtensions
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Volunteer_website.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using AutoMapper;

namespace Volunteer_website.Areas.Organization.Controllers
{
    [Area("Organization")]
    [Route("[area]/[controller]/[action]")] // Sửa lại route template
    [Authorize("Org")]
    public class ProfileController : Controller
    {
        private readonly VolunteerManagementContext _db;

        public ProfileController(VolunteerManagementContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return Unauthorized("User not authenticated.");
            }

            var profile = await _db.Organizations
                .FirstOrDefaultAsync(o => o.OrgId == orgId);

            if (profile == null)
            {
                return NotFound("Organization profile not found.");
            }

            return View(profile);
        }

        public async Task<IActionResult> EditProfile()
        {
            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return Unauthorized("User not authenticated.");
            }

            var profile = await _db.Organizations
                .FirstOrDefaultAsync(o => o.OrgId == orgId);

            if (profile == null)
            {
                return NotFound("Organization profile not found.");
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Volunteer_website.Models.Organization model, IFormFile? profileImage)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgId))
            {
                return Unauthorized("Người dùng chưa được xác thực.");
            }

            var profile = await _db.Organizations
                .FirstOrDefaultAsync(o => o.OrgId == orgId);

            if (profile == null)
            {
                return NotFound("Không tìm thấy hồ sơ tổ chức.");
            }

            // Kiểm tra điều kiện nhập liệu
            if (!InputValidator.IsValidString(model.Name))
            {
                ModelState.AddModelError("Name", "Tên phải chỉ chứa chữ cái và khoảng trắng.");
                return View(model);
            }

            if (!InputValidator.IsValidPhoneNumber(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại phải từ 10-11 chữ số và có thể bắt đầu bằng +.");
                return View(model);
            }

            if (!InputValidator.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Định dạng email không hợp lệ.");
                return View(model);
            }

            // Kiểm tra trùng số điện thoại với các tổ chức khác
            var existingPhone = await _db.Organizations
                .Where(o => o.OrgId != orgId && o.PhoneNumber == model.PhoneNumber)
                .FirstOrDefaultAsync();
            if (existingPhone != null)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại đã được sử dụng bởi một tổ chức khác.");
                return View(model);
            }
            // Kiểm tra trùng số điện thoại với các tổ chức khác
            var existingPhoneInVol = await _db.Volunteers
                .Where(o => o.VolunteerId != orgId && o.PhoneNumber == model.PhoneNumber)
                .FirstOrDefaultAsync();
            if (existingPhone != null)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại đã được sử dụng ");
                return View(model);
            }
          
            // Kiểm tra trùng email với các tổ chức khác
            var existingEmail = await _db.Organizations
                .Where(o => o.OrgId != orgId && o.Email == model.Email)
                .FirstOrDefaultAsync();
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng ");
                return View(model);
            }

            // Kiểm tra trùng email với các tình nguỵen khác
            var existingEmailInVol = await _db.Volunteers
                .Where(o => o.VolunteerId != orgId && o.Email == model.Email)
                .FirstOrDefaultAsync();
            if (existingEmailInVol != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng ");
                return View(model);
            }

            // Kiểm tra trùng email với các tổ chức khác
            var existingEmailInAdmin = await _db.Admins
                .Where(o => o.AdminId != orgId && o.Email == model.Email)
                .FirstOrDefaultAsync();
            if (existingEmailInAdmin != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng ");
                return View(model);
            }

            // Xử lý ảnh đại diện nếu có
            if (profileImage != null && profileImage.Length > 0)
            {
                string imagePath = await UpLoadImgService.UploadImg(profileImage, "organizations");
                if (imagePath != null)
                {
                    if (!InputValidator.IsValidImagePath(imagePath))
                    {
                        ModelState.AddModelError("", "Định dạng ảnh không hợp lệ. Chỉ cho phép .jpg, .jpeg, .png, hoặc .gif.");
                        return View(model);
                    }
                    profile.ImagePath = imagePath; // Lưu đường dẫn ảnh
                }
                else
                {
                    ModelState.AddModelError("", "Tải lên ảnh đại diện thất bại.");
                    return View(model);
                }
            }

            // Cập nhật thông tin
            profile.Name = model.Name;
            profile.PhoneNumber = model.PhoneNumber;
            profile.Address = model.Address;
            profile.Email = model.Email;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangePassword()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (id == null)
            {
                return NotFound();
            }

            var org = await _db.Organizations.FindAsync(id);
            if (org == null)
            {
                return NotFound();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string newUsername, string newPassword, string currentPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không trùng khớp!";
                return View();
            }
            if (string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(newUsername))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập các thông tin muốn cập nhật!";
                return View();
            }
            if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu có ít nhất 6 ký tự";
                return View();
            }

            var orgID = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(orgID))
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            var currentUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == orgID);
            var currentOrg = await _db.Organizations.FirstOrDefaultAsync(u => u.OrgId == orgID);

            if (currentOrg == null)
            {
                return NotFound("Không tìm thấy thông tin tổ chức.");
            }

            if (currentUser == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng trong hệ thống.");
            }

            var isNotValidUserNameUsers = await _db.Users
                .Where(e => e.UserId != currentOrg.OrgId)
                .AnyAsync(e => e.UserName == newUsername);
            if (isNotValidUserNameUsers && !string.IsNullOrEmpty(newUsername))
            {
                TempData["ErrorMessage"] = "UserName đã được sử dụng!";
                return View();
            }

            if (currentUser.RandomKey == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu người dùng không hợp lệ.";
                return View();
            }

            if (currentUser.Password == currentPassword.ToMd5Hash(currentUser.RandomKey))
            {
                if (!string.IsNullOrEmpty(newPassword))
                {
                    currentUser.RandomKey = Util.GenerateRandomkey();
                    currentUser.Password = newPassword.ToMd5Hash(currentUser.RandomKey);
                }
                if (!string.IsNullOrEmpty(newUsername))
                {
                    currentUser.UserName = newUsername;
                }

                _db.Users.Update(currentUser);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại bạn đã nhập sai. Vui lòng thử lại!";
            }
            return View();
        }

        #region Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "" });
        }
        #endregion
    }
}