using System.Linq;
using System.Text.RegularExpressions;

namespace Platformex.Web.Swagger
{
    public static class NamingUtils
    {
        public static string ToKebabCase(string pascalCasedString)
        {
            return Regex.Replace(pascalCasedString, "[a-z][A-Z]", m => $"{m.Value[0]}-{m.Value[1]}").ToLower();
        }

        public static string Humanize(string pascalCasedString)
        {
            return Regex.Replace(pascalCasedString, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

        public static string HumanizeTitle(string pascalCasedString)
        {
            return Regex.Replace(pascalCasedString, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}");
        }

        public static string ToPascalCase(string kebabCasedString)
        {
            return string.Join("", kebabCasedString.Split('-').Select(part => part.ToLower()).Select(part => part.Substring(0, 1).ToUpper() + part.Substring(1)));
        }
    }
}
