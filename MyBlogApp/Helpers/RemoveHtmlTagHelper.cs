//this was used to remove html tags from the content when displaying on the home page or other pages where we want to show plain text as the content we write is converted to html format
//so to remove those tag we created this own helper class
using System.Text.RegularExpressions;

namespace MyBlogApp.Helpers
{
    public static class RemoveHtmlTagHelper
    {
        public static string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Replace <p>, <br>, <div> with line breaks to preserve paragraph spacing
            input = Regex.Replace(input, @"<(p|br|div).*?>", "\n", RegexOptions.IgnoreCase);

            // Remove all other HTML tags
            string result = Regex.Replace(input, "<.*?>", string.Empty);

            // Normalize multiple consecutive line breaks
            result = Regex.Replace(result, @"\n\s*\n", "\n\n");

            // Trim leading/trailing whitespace
            return result.Trim();
        }
    }
}
