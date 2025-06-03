using System;
using System.Text.RegularExpressions;

namespace Volunteer_website.Helpers
{
    public static class InputValidator
    {
        // Kiểm tra chuỗi có phải là chữ (hỗ trợ Unicode, bao gồm tiếng Việt)
        public static bool IsValidString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            // Cho phép chữ cái Unicode (bao gồm tiếng Việt) và khoảng trắng
            return Regex.IsMatch(input, @"^[\p{L}\s]+$");
        }

        // Kiểm tra chuỗi có phải là số (chỉ chứa chữ số)
        public static bool IsValidNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return Regex.IsMatch(input, @"^\d+$");
        }

        // Kiểm tra số điện thoại (theo định dạng phổ biến, ví dụ: 10-11 chữ số)
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            return Regex.IsMatch(phoneNumber, @"^[\+]?[0-9]{10,11}$");
        }

        // Kiểm tra email hợp lệ
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Kiểm tra số tiền hợp lệ (decimal, không âm)
        public static bool IsValidAmount(decimal? amount)
        {
            return amount.HasValue && amount >= 0;
        }

        // Kiểm tra ngày hợp lệ (không null và không ở tương lai nếu cần)
        public static bool IsValidDate(DateOnly? date, bool allowFuture = true)
        {
            if (!date.HasValue)
                return false;
            if (!allowFuture && date.Value > DateOnly.FromDateTime(DateTime.Now))
                return false;
            return true;
        }

        // Kiểm tra ID không rỗng và theo định dạng (ví dụ: chỉ chữ và số)
        public static bool IsValidId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            return Regex.IsMatch(id, @"^[a-zA-Z0-9]+$");
        }

        // Kiểm tra trạng thái (status) có nằm trong danh sách cho phép
        public static bool IsValidStatus(string status, params string[] allowedStatuses)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;
            return allowedStatuses.Contains(status);
        }

        // Kiểm tra mật khẩu (ít nhất 8 ký tự, có chữ hoa, chữ thường, số)
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
        }

        // Kiểm tra giới tính (true: Male, false: Female)
        public static bool IsValidGender(bool? gender)
        {
            return gender.HasValue;
        }

        // Kiểm tra số lượng mục tiêu (TargetMember, TargetFunds) không âm
        public static bool IsValidTarget(int? target)
        {
            return target.HasValue && target >= 0;
        }

        // Kiểm tra đường dẫn ảnh (ví dụ: định dạng .jpg, .png)
        public static bool IsValidImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return false;
            return Regex.IsMatch(imagePath, @"\.(jpg|jpeg|png|gif)$", RegexOptions.IgnoreCase);
        }


        // Kiểm tra EvaluationId (bắt đầu bằng EVL, theo sau là số)
        public static bool IsValidEvaluationId(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && Regex.IsMatch(id, @"^EVL\d{4}$");
        }

        // Kiểm tra Feedback (chữ cái, số, khoảng trắng, dấu chấm, phẩy, hỗ trợ tiếng Việt)
        public static bool IsValidFeedback(string feedback)
        {
            return !string.IsNullOrEmpty(feedback) && Regex.IsMatch(feedback, @"^[\p{L}\p{N}\s.,]+$");
        }
    }
}