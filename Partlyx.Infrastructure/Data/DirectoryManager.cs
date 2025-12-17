namespace Partlyx.Infrastructure.Data
{
    public static class DirectoryManager
    {
        public static string PartlyxDataDirectory => FolderPathInAppData("Partlyx");
        public static string DefaultDBPath => Path.Combine(PartlyxDataDirectory, "partlyx.db");
        public static string DefaultSettingsDBPath => Path.Combine(PartlyxDataDirectory, "settings.db");

        public static bool CreatePartlyxFolder()
        {
            if (Directory.Exists(PartlyxDataDirectory)) return false;

            Directory.CreateDirectory(PartlyxDataDirectory);
            return true;
        }

        public static bool CreateAppDataFolder(string name)
        {
            var path = Path.Combine(DefaultDBPath, name);
            if (Directory.Exists(path)) return false;

            Directory.CreateDirectory(path);
            return true;
        }

        public static string FolderPathInAppData(string folder)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folder);
        }

        public static string GetInPartlyxFolder(string file)
        {
            return Path.Combine(PartlyxDataDirectory, file);
        }
    }
}
