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
                return Unauthorized("User not authenticated.");
            }

            var profile = await _db.Organizations
                .FirstOrDefaultAsync(o => o.OrgId == orgId);

            if (profile == null)
            {
                return NotFound("Organization profile not found.");
            }

            // Kiểm tra điều kiện nhập liệu
            if (!InputValidator.IsValidString(model.Name))
            {
                ModelState.AddModelError("Name", "Name must contain only letters and spaces.");
                return View(model);
            }

            if (!InputValidator.IsValidPhoneNumber(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must be 10-11 digits and may start with +.");
                return View(model);
            }

            if (!InputValidator.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format.");
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
                        ModelState.AddModelError("", "Invalid image format. Only .jpg, .jpeg, .png, or .gif are allowed.");
                        return View(model);
                    }
                    profile.ImagePath = imagePath; // Lưu đường dẫn ảnh
                }
                else
                {
                    ModelState.AddModelError("", "Failed to upload profile image.");
                    return View(model);
                }
            }

            // Cập nhật thông tin
            profile.Name = model.Name;
            profile.PhoneNumber = model.PhoneNumber;
            profile.Address = model.Address;
            profile.Email = model.Email;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(Change_Password model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Role == 2);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Kiểm tra điều kiện nhập liệu cho mật khẩu
            if (!InputValidator.IsValidPassword(model.OldPassword))
            {
                ModelState.AddModelError("OldPassword", "Old password must be at least 8 characters with uppercase, lowercase, and numbers.");
                return View(model);
            }

            if (!InputValidator.IsValidPassword(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New password must be at least 8 characters with uppercase, lowercase, and numbers.");
                return View(model);
            }

            string hashedCurrentPassword = model.OldPassword?.ToSHA256Hash(user.RandomKey) ?? string.Empty;
            if (hashedCurrentPassword != user.Password)
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("", "New password cannot be null or empty.");
                return View(model);
            }

            string newRandomKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(128 / 8));
            string newHashedPassword = model.NewPassword.ToSHA256Hash(newRandomKey);

            user.Password = newHashedPassword;
            user.RandomKey = newRandomKey;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(Index));
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