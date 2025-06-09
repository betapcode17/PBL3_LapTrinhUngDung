using System.Text;
using System.Text.RegularExpressions;

namespace Volunteer_website.Helpers
{
    public class Util
    {
        public static string GenerateRandomkey(int length = 5)
        {
            var pattern = @"qwertyuiopasdfghjklzxcvbnm";
            var sb = new StringBuilder();
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0,pattern.Length)]);
            }

            return sb.ToString();
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            var regex = new Regex("^(0?)(3[2-9]|5[6|8|9]|7[0|6-9]|8[0-6|8|9]|9[0-4|6-9])[0-9]{7}$");
            return regex.IsMatch(phoneNumber);
        }

        internal static bool IsValidEmail(string v)
        {
            throw new NotImplementedException();
        }
    }
}
