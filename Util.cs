using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BrusLib {
    public static class Util {
        public const string SYNERGIA_INDEX = "https://synergia.librus.pl/uczen/index";

        public static string Cfg_BufferLocation;
        
        //TODO: move generic LibrusAuth methods here, to the util class

        public static async Task<string> FetchAsync(string url, CookieContainer cookies, string referer) {
            var request = GetRequest(url, ref cookies, true, referer);
            var response = await request.GetResponseAsync();
            return LibrusAuth.GetResponseBody(response);
        }
        
        public static async Task<string> FetchAsyncPost(string url, CookieContainer cookies, string referer, string post) {
            var request = GetRequest(url, ref cookies, true, referer);
            MakePostRequest(ref request, post);
            var response = await request.GetResponseAsync();
            return LibrusAuth.GetResponseBody(response);
        }
        
        public static string Fetch(string url, ref CookieContainer cookies, string referer) {
            var request = GetRequest(url, ref cookies, true, referer);
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

        /// <summary>
        /// Makes the WebRequest into a POST type request with specified content
        /// </summary>
        /// <param name="request">A reference to the request</param>
        /// <param name="content">The content to write</param>
        public static void MakePostRequest(ref HttpWebRequest request, string content) {
            byte[] data = Encoding.UTF8.GetBytes(content);
            
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = data.Length;
            request.Accept = "*/*";
            
            Stream str = request.GetRequestStream();
            str.Write(data, 0, data.Length);
            str.Close();
        }
        
        /// <summary>
        /// Sets the common headers for all web requests
        /// </summary>
        /// <param name="request">A reference to the web request</param>
        private static void SetDefaultHeaders(ref HttpWebRequest request) {
            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8";
            request.Headers.Add("Sec-Fetch-Dest","document");
            request.Headers.Add("Sec-Fetch-Mode","navigate");
            request.Headers.Add("Sec-Fetch-Site","same-site");
            request.Headers.Add("Sec-Fetch-User","?1");
            request.Headers.Add("Sec-GPS", "1");
            request.Headers.Add("DNT", "1");
        }

        /// <summary>
        /// Creates a request in an elegant way
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="cookies">A reference to the cookie container of the session</param>
        /// <param name="setDefaultHeaders">Shall the function set the def. headers?</param>
        /// <param name="referer">Setter for the commonly used referer header.</param>
        /// <returns>The created HttpWebRequest</returns>
        public static HttpWebRequest GetRequest(string url, ref CookieContainer cookies,  bool setDefaultHeaders = true, string referer = "") {
            var wr = WebRequest.CreateHttp(url);
            wr.CookieContainer = cookies;
            
            if(referer != "") wr.Referer = referer;
            if(setDefaultHeaders) SetDefaultHeaders(ref wr);
            return wr;
        }
    }
}