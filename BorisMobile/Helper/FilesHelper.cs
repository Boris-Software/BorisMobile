namespace BorisMobile.Helper
{
    public class FilesHelper
    {
        public static string GetConfigDirectoryMAUI()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "config");
        }
        public static string GetAttachmentDirectoryMAUI(string appName)
        {
            return Path.Combine(FileSystem.AppDataDirectory, "attachments");
        }
        public static string GetImagesDirectoryMAUI()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "images");
        }
        public static string GetCurrentDirectory()
        {
            return Path.Combine(FileSystem.AppDataDirectory);
        }
    }
}
