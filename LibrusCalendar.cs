using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusCalendar {
        public readonly Dictionary<DateTime, List<SchoolEvent>> events;
        
        public LibrusCalendar(Dictionary<DateTime, List<SchoolEvent>> events) {
            this.events = events;
        }

        public static async Task<LibrusCalendar> Retrieve(LibrusConnection connection) {
            string html = await Util.FetchAsync("https://synergia.librus.pl/terminarz", connection.cookieSession,
                Util.SYNERGIA_INDEX);
            
            //File.WriteAllText("terminarz req.html",html);
            /* File.ReadAllText("terminarz req.html");*/
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var calRoot = document.SelectSingleNode("/html/body/div[3]/div[3]/form/div/div/div/table");
            var days = calRoot.SelectNodes("//div[@class=\"kalendarz-dzien\"]");

            var results = new Dictionary<DateTime, List<SchoolEvent>>();
            
            foreach (var day in days) {
                var ls = new List<SchoolEvent>();
                int d = int.Parse(day.SelectSingleNode("./div").InnerText);
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, d); // huge mess; TODO: cleanup?
                
                var events = day.SelectNodes(".//tr");
                if(events == null) continue;
                foreach (var e in events) {
                    string it = e.SelectSingleNode("./td").InnerHtml;
                    string tt = e.SelectSingleNode("./td").GetAttributeValue("title", "");
                    ls.Add(IdentifyEvent(it,tt,date ));
                }
                results.Add(date, ls);
            }
            
            return new LibrusCalendar(results);
        }

        private static SchoolEvent IdentifyEvent(string innerText, string descriptionText, DateTime w) {
            if (innerText.Contains("Praca klasowa") || innerText.Contains("Kartkówka") || innerText.Contains("Inne")) {
                return new SEExam(innerText + "<br>" + descriptionText, w);
            }
            else
                return new SchoolEvent(innerText + "<br>" + descriptionText, w);
        }

        public List<SchoolEvent> GetAllEvents() {
            List<SchoolEvent> es = new List<SchoolEvent>();
            foreach (var i in events) {
                es.AddRange(i.Value);
            }

            return es;
        }
        
        
        
        
    }
}