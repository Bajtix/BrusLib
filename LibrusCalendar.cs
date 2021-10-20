﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusCalendar {
        private const string requestUrl = "https://synergia.librus.pl/terminarz";

        public Dictionary<DateTime, CalendarEvent> events;

        private LibrusCalendar(Dictionary<DateTime, CalendarEvent> events) {
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

            var resultDictionary = new Dictionary<DateTime, CalendarEvent>();
            
            foreach (var d in days) {
                var day = d.SelectSingleNode("./div[@class=\"kalendarz-numer-dnia\"]");
                var events = d.SelectNodes(".//td");
                
                if(events == null) continue;
                foreach (var e in events) {
                    //Console.WriteLine(e.InnerHtml);
                    string url = e.GetAttributeValue("onclick", "null");
                    var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(day.InnerText));
                    if (url != "null") {
                        url = url.Split('\'')[1].Replace("'","");
                        var ce = new CalendarEvent(e.InnerText, "https://synergia.librus.pl" + url);
                        resultDictionary.Add(date, ce);
                    }
                }
                
                
            }
            
            return new LibrusCalendar(resultDictionary);
        }
    }
}