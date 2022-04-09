using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BrusLib2 {
    public class LibrusDataProvider : DataProvider {
        private WebCommunicator m_web = new();
        private HtmlDocument m_document = new();



        ///<summary>Gets the code from the iframe in step 1</summary>
        private string GetIframeSource(string body) {
            if (body == string.Empty) return ""; //TODO: throw the appropriate exception.
            m_document.LoadHtml(body);
            string iframeSource = m_document.DocumentNode.SelectSingleNode("//iframe[@id=\"caLoginIframe\"]").GetAttributeValue("src", "ERROR");
            if (iframeSource == "ERROR") return ""; //TODO: throw the appropriate exception.
            return iframeSource;
        }

        public override bool Login(string username, string password) {
            //step 1: get the client code from the frame
            var iframeCode = GetIframeSource(
                m_web.GetRequest("https://portal.librus.pl/rodzina/synergia/loguj", "https://portal.librus.pl/rodzina")
                .GetResponse()
                .GetResponseBody()
            ); // TODO: probably needs to be async.

            var authRefererUri = m_web.GetRequest(iframeCode, "https://portal.librus.pl/rodzina").GetResponse().ResponseUri.ToString();

            // greet the captcha
            m_web.PostRequest("https://api.librus.pl/OAuth/Captcha", "username=&is_needed=1", authRefererUri).GetResponse();

            Thread.Sleep(10);

            // feed the captcha
            m_web.PostRequest("https://api.librus.pl/OAuth/Captcha", $"username={username}&is_needed=1", authRefererUri).GetResponse();

            var finalResponse = m_web.PostRequest(authRefererUri, $"action=login&login={username}&pass={password}", authRefererUri).GetResponse().GetResponseBody();

            var response = JsonConvert.DeserializeObject<dynamic>(finalResponse);
            if (response == null) return false;
            if (!response!.status)
                return false;
            if (response!.status != "ok")
                return false;

            m_web.GetRequest(authRefererUri.Replace("Authorization", "Authorization/Grant"), authRefererUri).GetResponse();

            return true;
        }
    }
}
