using System.Net;
using System.Threading.Tasks;

namespace BrusLib {
    public class Util {
        public const string SYNERGIA_INDEX = "https://synergia.librus.pl/uczen/index";
        
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
    }
}