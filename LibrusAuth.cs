using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BrusLib {
    public static class LibrusAuth {
        public class AuthEvent {
            public string message;
            public Exception e;
            public DateTime occured;
            public AuthEvent(string message, Exception e, DateTime occured) {
                this.message = message;
                this.e = e;
                this.occured = occured;
            }
        }

        public static float delayMultiplier = 1;
        

        /// <summary>
        /// Retrieves the response stream from WebResponse
        /// </summary>
        /// <returns>Response Content</returns>
        public static string GetResponseBody(WebResponse r) {
            StreamReader sr = new StreamReader(r.GetResponseStream());
            string o = sr.ReadToEnd();
            sr.Close();
            //File.WriteAllText("latest_request.txt", o);
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
        /// Checks if the server response is OK in step 5
        /// </summary>
        private static bool IsResponseOk(string response) {
            string status = response; // better to do json convert
            if (status == null) status = "null status";
            
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
        public static async Task<LibrusConnection> Authenticate(string username, string password, EventHandler<AuthEvent> eventHandler) {
            LibrusConnection connection = new LibrusConnection(username, password, new CookieContainer());

            HttpWebRequest request;
            WebResponse response;
            string referer; // stores the client_id for the referer url. should look something like this https://api.librus.pl/OAuth/Authorization?client_id=XX
            
            // Step 1
            // gets the iframe code
            try {
                request = Util.GetRequest("https://portal.librus.pl/rodzina/synergia/loguj", ref connection.cookieSession,
                    true, "https://portal.librus.pl/rodzina");
                response = request.GetResponse();
            }
            catch (Exception e) {
                eventHandler.Invoke(null, new AuthEvent("Step 1 Failed", e, DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED","EXCEPTION_FAILED", null, false);
            }

            eventHandler.Invoke(null, new AuthEvent("Step 1 : Get iframe code", null, DateTime.Now));


            
            
            // Step 2
            // gets us the Auth Referer url and go there
            string iframeCode = GetIframeCode(GetResponseBody(response));
            try {
                request = Util.GetRequest(iframeCode, ref connection.cookieSession, true,
                    "https://portal.librus.pl/rodzina");
                response = request.GetResponse();
                referer = response.ResponseUri.ToString();
            }
            catch (Exception e) {
                eventHandler.Invoke(null, new AuthEvent("Step 2 Failed", e, DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED","EXCEPTION_FAILED", null, false);
            }

            eventHandler.Invoke(null, new AuthEvent("Step 2 : Get auth referer url", null, DateTime.Now));

            
            
            // Step 3
            // Greet the captcha (the system they use is kinda dumb - to trick it, we first send an empty username and then a filled one)
            try {
                request = Util.GetRequest("https://api.librus.pl/OAuth/Captcha", ref connection.cookieSession, true,
                    referer);
                Util.MakePostRequest(ref request, "username=&is_needed=1");
                response = request.GetResponse();
            }
            catch (Exception e) {
                eventHandler.Invoke(null, new AuthEvent("Step 3 Failed", e, DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED","EXCEPTION_FAILED", null, false);
            }

            eventHandler.Invoke(null, new AuthEvent("Step 3 : Greet the captcha", null, DateTime.Now));

            
            
            // We need to wait here (i think, sometimes it wouldn't work otherwise, which makes sense - a user wouldn't type his password in just a few ms)
            await Task.Delay((int)(100 * delayMultiplier));
            
            // Step 4
            // Feed the captcha
            try {
                request = Util.GetRequest("https://api.librus.pl/OAuth/Captcha", ref connection.cookieSession, true,
                    referer);
                Util.MakePostRequest(ref request, $"username={username}&is_needed=1");
                response = request.GetResponse();
            }
            catch (Exception e) {
                eventHandler.Invoke(null, new AuthEvent("Step 4 Failed", e, DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED","EXCEPTION_FAILED", null, false);
            }

            eventHandler.Invoke(null, new AuthEvent("Step 4 : Feed the captcha", null, DateTime.Now));


            await Task.Delay((int)(100 * delayMultiplier));
            
            // Step 5
            // finally, we send the credentials. This will get us some json - we care about the status part (check if its 'ok')

            try {
                request = Util.GetRequest(referer, ref connection.cookieSession, true, referer);
                Util.MakePostRequest(ref request, $"action=login&login={username}&pass={password}");
                response = request.GetResponse();
            }
            catch (Exception e) {
                eventHandler.Invoke(null, new AuthEvent("Step 5 Failed", e, DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED","EXCEPTION_FAILED", null, false);
            }

            eventHandler.Invoke(null, new AuthEvent("Step 5 : Send credentials", null, DateTime.Now));


            string _ = GetResponseBody(response);
            if (!IsResponseOk(_)) {
                eventHandler.Invoke(null, new AuthEvent("Step 5 Verification Failed", new Exception($"Server told us to fuck off. Response: {_}"), DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED","EXCEPTION_FAILED", null, false);
            }

            // Step 6
            // Now the server will send us some json back. We just have to follow to the next page
            try {
                request = Util.GetRequest(
                    referer.Replace("Authorization", "Authorization/Grant"),
                    ref connection.cookieSession, true, referer);
                response = request.GetResponse();
            }
            catch (Exception e) {
                eventHandler.Invoke(null, new AuthEvent("Step 6 Failed", e, DateTime.Now));
                return new LibrusConnection("EXCEPTION_FAILED", "EXCEPTION_FAILED", null, false);
            }

            eventHandler.Invoke(null, new AuthEvent("Step 6 : Login successful", null, DateTime.Now));





            return connection;
        }
        
        
    }
}