using System.Text.RegularExpressions;

namespace BrusLib2;

public static class HtmlUtils {
    public static string HTMLReformatLineBreaks(this string s) {
        return s.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<BR>", "\n").Replace("<BR/>", "\n").Replace("<BR />", "\n");
    }

    //a function that returns a string with all the html tags removed       
    public static string HTMLRemoveTags(this string s) {
        return Regex.Replace(s, @"<(.|\n)*?>", string.Empty);
    }
}