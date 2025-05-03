using System.Text.RegularExpressions;

namespace SecureDocumentExchange.Web.Helpers
{
    public static class Sanitizer
    {
        public static string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Remove script tags and dangerous characters
            input = Regex.Replace(input, @"<script.*?>.*?</script>", "", RegexOptions.IgnoreCase);
            input = input.Replace("<", "").Replace(">", "").Replace("\"", "").Replace("'", "");
            return input.Trim();
        }
    }
}
