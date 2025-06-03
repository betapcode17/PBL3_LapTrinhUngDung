using System.Text;

namespace Volunteer_website.Helpers
{
    public class UpLoadImgService
    {

        public static async Task<string> UploadImg(IFormFile imageFile, string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            try
            {
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string folderPath = Path.Combine(webRootPath, "images", folderName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                fileName = RemoveInvalidChars(fileName);
                fileName += Path.GetExtension(imageFile.FileName);

                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                string link = Path.Combine("images", folderName, fileName).Replace("\\", "/");
                if (link.Substring(0, 1) == "/")
                    return link;
                else
                    return "/" + link;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return null;
            }
        }
        public static async Task<List<string>> UploadListImg(IFormFileCollection imageFiles, string folderName)
        {
            var uploadedPaths = new List<string>();

            if (imageFiles == null || imageFiles.Count == 0)
            {
                return uploadedPaths;
            }

            try
            {
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string folderPath = Path.Combine(webRootPath, "images", folderName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                        fileName = RemoveInvalidChars(fileName);

                      
                        fileName += "_" + DateTime.Now.Ticks + Path.GetExtension(imageFile.FileName);

                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        string relativePath = Path.Combine("images", folderName, fileName).Replace("\\", "/");
                        if (relativePath.Substring(0, 1) != "/")
                            relativePath = "/" + relativePath;

                        uploadedPaths.Add(relativePath);
                    }
                }

                return uploadedPaths;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return uploadedPaths;
            }
        }

        private static string RemoveInvalidChars(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c.ToString(), "");
            }

            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(name);
            name = Encoding.ASCII.GetString(bytes);

            return name;
        }
    }
}
