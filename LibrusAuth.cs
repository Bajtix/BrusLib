using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BrusLib {
    public static class LibrusAuth {
        
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
            
            if(referer != "") wr.Headers.Add(HttpRequestHeader.Referer, referer);
            if(setDefaultHeaders) SetDefaultHeaders(ref wr);
            return wr;
        }
        
        /// <summary>
        /// Retrieves the response stream from WebResponse
        /// </summary>
        /// <returns>Response Content</returns>
        public static string GetResponseBody(WebResponse r) {
            StreamReader sr = new StreamReader(r.GetResponseStream()!);
            string o = sr.ReadToEnd();
            sr.Close();
            File.WriteAllText("latest_request.txt", o);
            return o;
        }

        /// <summary>
        /// Gets the code from the iframe in step 1
        /// </summary>
        private static string GetIframeCode(string body) {
            if (body == String.Empty) throw new Exception("Failed getting Iframe code - body empty");
            HtmlDocument w = new HtmlDocument();
            w.LoadHtml(body);
            string r = w.DocumentNode.SelectSingleNode("//iframe[@id=\"caLoginIframe\"]").GetAttributeValue("src", "ERROR");
            if (r == "ERROR") throw new Exception("Failed getting Iframe code - can't find");
            return r;
        }
        
        /// <summary>
        /// Makes the WebRequest into a POST type request with specified content
        /// </summary>
        /// <param name="request">A reference to the request</param>
        /// <param name="content">The content to write</param>
        private static void MakePostRequest(ref HttpWebRequest request, string content) {
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
        /// Checks if the server response is OK in step 5
        /// </summary>
        private static bool IsResponseOk(string response) {
            string status = JsonConvert.DeserializeObject<dynamic>(response)?.status;
            status ??= "null status";
            
            if (status.Contains("ok")) return true;
            Console.WriteLine($"Error while verifying response. It's not ok - got {status}");
            return false;
        }
        
        /// <summary>
        /// Creates a Connection for the provided credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Connection session with all the cookies set etc.</returns>
        public static async Task<LibrusConnection> Authenticate(string username, string password, bool verbose = false) {
            LibrusConnection connection = new LibrusConnection(username, new CookieContainer());

            HttpWebRequest request;
            WebResponse response;
            string referer; // stores the client_id for the referer url. should look something like this https://api.librus.pl/OAuth/Authorization?client_id=XX
            
            // Step 1
            // gets the iframe code
            request = GetRequest("https://portal.librus.pl/rodzina/synergia/loguj", ref connection.cookieSession, true, "https://portal.librus.pl/rodzina");
            response = request.GetResponse();
            
            if(verbose) Console.WriteLine("Step 1 : Get iframe code");
            
            // Step 2
            // gets us the Auth Referer url
            string iframeCode = GetIframeCode(GetResponseBody(response));
            request = GetRequest(iframeCode, ref connection.cookieSession, true, "https://portal.librus.pl/rodzina");
            response = request.GetResponse();
            referer = response.ResponseUri.ToString();
            
            if(verbose) Console.WriteLine("Step 2 : Get auth referer url");
            
            // Step 3
            // Greet the captcha (the system they use is kinda dumb - to trick it, we first send an empty username and then a filled one)
            request = GetRequest("https://api.librus.pl/OAuth/Captcha", ref connection.cookieSession, true, referer);
            MakePostRequest(ref request, "username=&is_needed=1");
            response = request.GetResponse();
            
            if(verbose) Console.WriteLine("Step 3 : Greet the captcha");
            
            // We need to wait here (i think, sometimes it wouldn't work otherwise, which makes sense - a user wouldn't type his password in just a few ms)
            await Task.Delay(500);
            
            // Step 4
            // Feed the captcha
            request = GetRequest("https://api.librus.pl/OAuth/Captcha", ref connection.cookieSession, true, referer);
            MakePostRequest(ref request, $"username={username}&is_needed=1");
            response = request.GetResponse();
            
            if(verbose) Console.WriteLine("Step 4 : Feed the captcha");
            
            await Task.Delay(500);
            
            // Step 5
            // finally, we send the credentials. This will get us some json - we care about the status part (check if its 'ok')
            request = GetRequest(referer, ref connection.cookieSession, true, referer);
            MakePostRequest(ref request, $"action=login&login={username}&pass={password}");
            response = request.GetResponse();
            
            if(verbose) Console.WriteLine("Step 5 : Send Credentials");
            
            string _ = GetResponseBody(response);
            if (!IsResponseOk(_)) throw new Exception("Credentials error: " + _);

            // Step 6
            // Now the server will send us some json back. We just have to follow to the next page
            request = GetRequest(
                referer.Replace("Authorization", "Authorization/Grant"), 
                ref connection.cookieSession, true, referer);
            response = request.GetResponse();
            
            if(verbose)  Console.WriteLine("Step 6 : Got full access!");
            

            return connection;
        }
        
        
    }
}