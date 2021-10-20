using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusCalendar {
        private const string requestUrl = "https://synergia.librus.pl/terminarz";

        public Dictionary<DateTime, List<CalendarEvent>> events;

        private LibrusCalendar(Dictionary<DateTime, List<CalendarEvent>> events) {
            this.events = events;
        }

        public static async Task<LibrusCalendar> Retrieve(LibrusConnection connection, APIBufferMode bufferMode = APIBufferMode.none) {
            string html = "";

            switch (bufferMode) {
                case APIBufferMode.none:
                    html = await Util.FetchAsync(requestUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    break;
                case APIBufferMode.load:
                    if (File.Exists("buffer_calendar")) {
                        html = File.ReadAllText("buffer_calendar");
                        break;
                    }
                    else
                        goto case APIBufferMode.save;
                case APIBufferMode.save:
                    html = await Util.FetchAsync(requestUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    File.WriteAllText("buffer_calendar", html);
                    break;
            }
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var tableNode = document.SelectSingleNode("/html/body/div[3]/div[3]/form/div/div/div/table");
            var days = tableNode.SelectNodes(".//div[@class=\"kalendarz-dzien\"]");

            var resultDictionary = new Dictionary<DateTime, List<CalendarEvent>>();

            
            
            foreach (var d in days) {
                var day = d.SelectSingleNode("./div[@class=\"kalendarz-numer-dnia\"]");
                var events = d.SelectNodes(".//td");
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(day.InnerText));
                resultDictionary.Add(date, new List<CalendarEvent>());
                
                if(events == null) continue;
                foreach (var e in events) {
                    //Console.WriteLine(e.InnerHtml);
                    string url = e.GetAttributeValue("onclick", "null");
                    
                    if (url != "null" || url.Contains("wolne")) { //ignorujemy zwolnienia nauczycieli - 1. są nieprzydatne, 2. zapychają łącze, 3.nie chce mi się ich robić
                        url = url.Split('\'')[1].Replace("'","");
                        var ce = new CalendarEvent(e.InnerText, "https://synergia.librus.pl" + url);
                        if (resultDictionary.ContainsKey(date)) {
                            resultDictionary[date].Add(ce);
                        } else
                            resultDictionary.Add(date, new List<CalendarEvent>(){ce}); // shouldnt happen
                    }
                }
                
                
            }
            
            return new LibrusCalendar(resultDictionary);
        }
    }
}