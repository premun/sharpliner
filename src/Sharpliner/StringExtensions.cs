namespace Sharpliner;

internal static class StringExtensions
{
    public static string TrimStart(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString))
        {
            return target;
        }

        string result = target;
        while (result.StartsWith(trimString))
        {
            result = result[trimString.Length..];
        }

        return result;
    }

    public static string TrimEnd(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString))
        {
            return target;
        }

        string result = target;
        while (result.EndsWith(trimString))
        {
            result = result[..^trimString.Length];
        }

        return result;
    }
}
