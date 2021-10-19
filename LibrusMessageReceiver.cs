using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusMessageReceiver {
        private const string requestUrl = "https://synergia.librus.pl/wiadomosci/5/f0";
        
        public List<LibrusMessage> messages;
        
        private LibrusMessageReceiver(List<LibrusMessage> messages) {
            this.messages = messages;
        }

        public static async Task<LibrusMessageReceiver> Retrieve(LibrusConnection connection, APIBufferMode bufferMode = APIBufferMode.none) {
            string html = "";

            switch (bufferMode) {
                case APIBufferMode.none:
                    html = await Util.FetchAsync(requestUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    break;
                case APIBufferMode.load:
                    if (File.Exists("buffer_messages")) {
                        html = File.ReadAllText("buffer_messages");
                        break;
                    }
                    else
                        goto case APIBufferMode.save;

                case APIBufferMode.save:
                    html = await Util.FetchAsync(requestUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    File.WriteAllText("buffer_messages", html);
                    break;
            }
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var tableNode = document.SelectSingleNode(
                "//table[@class=\"decorated stretch\"]");

            List<LibrusMessage> messages = new List<LibrusMessage>();

            foreach (var row in tableNode.SelectNodes(".//tr").Skip(1) /*the first row contains definitions*/) {
                var elements = row.SelectNodes(".//td");
                
                if(elements.Count < 5) continue;

                var messageAuthor = elements[2];
                var messageTitle = elements[3];
                var messageTime = elements[4];
                
                //Console.WriteLine($"{messageTime.InnerText} | {messageAuthor.InnerText} | {messageTitle.InnerText} |");
                messages.Add(new LibrusMessage(messageAuthor.InnerText.Trim(),
                    messageTitle.InnerText.Trim(), 
                    messageTime.InnerText.Trim(),
                    "https://synergia.librus.pl" + Util.DeHtmlify(messageTitle.SelectSingleNode("./a").GetAttributeValue("href",""))
                ));
            }
            return new LibrusMessageReceiver(messages);
        }
    }
}