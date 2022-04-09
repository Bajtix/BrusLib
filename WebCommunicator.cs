using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BrusLib2 {
    public class WebCommunicator {
        private CookieContainer m_cookies = new();
        private WebHeaderCollection m_headers = new();
        private HttpWebRequest? m_lastRequest;

        public HttpWebRequest? LastRequest => m_lastRequest;


        public void SetHeader(HttpRequestHeader header, string value) => m_headers.Set(header, value);

        /// <summary>Creates a request</summary>
        /// <param name="url">Request url</param>
        /// <param name="referer">Setter for the commonly used referer header.</param>
        /// <returns>The created HttpWebRequest</returns>
        public HttpWebRequest GetRequest(string url, string referer = "", bool setDefaultHeaders = true) {
            var request = WebRequest.CreateHttp(url); // TODO: remake this with the HttpClient classes. check out https://scrapingpass.com/blog/web-scraping-with-c/
            request.CookieContainer = m_cookies;
            request.Headers = m_headers;
            if (!string.IsNullOrWhiteSpace(referer)) request.Referer = referer;
            m_lastRequest = request; // this will just make it easier to use later, as we don't need to store the last request all the time
            return request;
        }

        public async Task<HttpWebResponse> SendGetRequest(string url, string referer = "", bool setDefaultHeaders = true) {
            var request = GetRequest(url, referer, setDefaultHeaders);
            return (HttpWebResponse)await request.GetResponseAsync();
        }

        public HttpWebRequest PostRequest(string url, string content, string referer = "", bool setDefaultHeaders = true) {
            var request = GetRequest(url, referer, setDefaultHeaders);

            byte[] data = Encoding.UTF8.GetBytes(content);

            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = data.Length;
            request.Accept = "*/*";

            Stream str = request.GetRequestStream();
            str.Write(data, 0, data.Length);
            str.Close();

            return request;
        }

        public async Task<HttpWebResponse> SendPostRequest(string url, string content, string referer = "", bool setDefaultHeaders = true) {
            var request = PostRequest(url, content, referer, setDefaultHeaders);
            return (HttpWebResponse)await request.GetResponseAsync();
        }
    }
}