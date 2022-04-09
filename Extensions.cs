using System.IO;
using System.Net;

namespace BrusLib2 {
    public static class Extensions {
        /// <summary>Retrieves the response stream from WebResponse</summary>
        /// <returns>Response content</returns>
        public static string GetResponseBody(this WebResponse r) {
            var sr = new StreamReader(r.GetResponseStream());
            string body = sr.ReadToEnd();
            sr.Close();
            return body;
        }
    }
}