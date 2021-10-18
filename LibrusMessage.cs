using System;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusMessage {
        public string Author { get; }
        public string Title { get; }
        public string Content { private set; get; }
        public DateTime ReceiveDate { get; }
        public string ContentUrl { get; }
        

        public LibrusMessage(string author, string title, string recieveDate, string contentUrl) {
            Author = author.Substring(0, author.IndexOf('(')-1); // trim the message not to include the dumb ( )
            Title = title;
            ReceiveDate = DateTime.Parse(recieveDate);
            ContentUrl = contentUrl;
        }

        public async Task<string> ReceiveContent(LibrusConnection connection, APIBufferMode bufferMode) {

            string furl = ContentUrl.Replace("/", "_").Replace(":","-");
            
            string html = "";

            switch (bufferMode) {
                case APIBufferMode.none:
                    html = await Util.FetchAsync(ContentUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    break;
                case APIBufferMode.load:
                    if (File.Exists("buffer_message_"+furl)) {
                        html = File.ReadAllText("buffer_message_"+furl);
                        break;
                    }
                    else
                        goto case APIBufferMode.save;

                case APIBufferMode.save:
                    html = await Util.FetchAsync(ContentUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    File.WriteAllText("buffer_message_"+furl, html);
                    break;
            }
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var messageDiv = document.SelectSingleNode("//div[@class=\"container-message-content\"]");

            Content = messageDiv.InnerText;
            return Content;
        }
    }
}