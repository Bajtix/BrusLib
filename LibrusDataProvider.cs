using System;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BrusLib2;

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

    // TODO: error handling
    public override async Task<bool> Login(string username, string password) {
        //step 1: get the client code from the frame
        var iframeCode = GetIframeSource(
            (await m_web.SendGetRequest("https://portal.librus.pl/rodzina/synergia/loguj", "https://portal.librus.pl/rodzina"))
            .GetResponseBody()
        ); // TODO: probably needs to be async.

        var authRefererUri = m_web.GetRequest(iframeCode, "https://portal.librus.pl/rodzina").GetResponse().ResponseUri.ToString();

        // greet the captcha
        await m_web.SendPostRequest("https://api.librus.pl/OAuth/Captcha", "username=&is_needed=1", authRefererUri);

        await Task.Delay(10);

        // feed the captcha
        await m_web.SendPostRequest("https://api.librus.pl/OAuth/Captcha", $"username={username}&is_needed=1", authRefererUri);

        var finalResponse = (await m_web.SendPostRequest(authRefererUri, $"action=login&login={username}&pass={password}", authRefererUri)).GetResponseBody(); // !!! if the password was incorrect, this throws 403

        var response = JsonConvert.DeserializeObject<dynamic>(finalResponse);
        if (response == null) return false;
        if (response.status != "ok")
            return false;

        await m_web.SendGetRequest(authRefererUri.Replace("Authorization", "Authorization/Grant"), authRefererUri);

        m_sessionValidity = DateTime.Now.AddMinutes(20); // could be 30, but it's just safer this way (and it's not a big deal if it's not exactly 30 minutes)

        return true;
    }

    public override async Task<Subject[]> FetchSubjectsGrades() {
        if (!IsSessionValid) throw new SessionExpiredException(m_sessionValidity);

        var html = (await m_web.SendGetRequest("https://synergia.librus.pl/przegladaj_oceny/uczen")).GetResponseBody();

        return LibrusDataParser.ParseHTMLSubjects(html);
    }
}