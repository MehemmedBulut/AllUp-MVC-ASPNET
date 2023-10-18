

namespace AllUp2.Helpers
{
    public static class Extensions
    {
        public static bool IsImage(this IFormFile file)
        {
            return file.ContentType.Contains("image/");
        }
        public static bool IsOlder1Mb(this IFormFile file)
        {
            return file.Length/1024>1024;
        }
        public static async Task<string> SaveFileAsync(this IFormFile file,string folder)
        {
            string fileName = Guid.NewGuid().ToString()+file.FileName;
            string fullPath = Path.Combine(folder,fileName);
            using (FileStream fileStream=new FileStream(fullPath,FileMode.Create)) 
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }
    }
}
