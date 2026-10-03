public static class PathExtensions
{
    public static string GetRootDirectory()
    {
        return Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))!;
    }
}
