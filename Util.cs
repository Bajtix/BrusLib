using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace BrusLib {
    public static class Util {
        public const string SYNERGIA_INDEX = "https://synergia.librus.pl/uczen/index";

        public static string Cfg_BufferLocation;
        
        //TODO: move generic LibrusAuth methods here, to the util class

        public static async Task<string> FetchAsync(string url, CookieContainer cookies, string referer) {
            var request = LibrusAuth.GetRequest(url, ref cookies, true, referer);
            var response = await request.GetResponseAsync();
            return LibrusAuth.GetResponseBody(response);
        }
        
        public static string Fetch(string url, ref CookieContainer cookies, string referer) {
            var request = LibrusAuth.GetRequest(url, ref cookies, true, referer);
            return LibrusAuth.GetResponseBody(request.GetResponse());
        }

        public static string DeHtmlify(string h) {
            h = h.Replace("&nbsp;", " ");
            h = h.Replace("&nbsp", " ");
            return HttpUtility.HtmlDecode(h);
        }
        
        public static DateTime GetFirstDayOfWeek(DateTime dayInWeek, CultureInfo cultureInfo) {
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dayInWeek.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }
    }
}