namespace BorisMobile.Helper
{
    public class FilesHelper
    {
        public static string GetConfigDirectoryMAUI()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "config");
        }
        public static string GetUploadsDirectoryMAUI()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "uploads");
        }

        public static string GetAttachmentDirectoryMAUI()
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
