namespace PdfUnlocker;

internal static class PasswordList
{
    public static IReadOnlyList<string> Load(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        return File.ReadAllLines(path)
            .Select(static line => line.Trim())
            .Where(static line => line.Length > 0 && !line.StartsWith('#'))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static string DefaultPath =>
        Path.Combine(AppContext.BaseDirectory, "passwords.txt");
}
