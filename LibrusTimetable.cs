﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusTimetable {
        
        

        public List<TimePeriod> timePeriods;

        public SchoolDay[] week;

        private const string requestUrl = "https://synergia.librus.pl/przegladaj_plan_lekcji";

        public LibrusTimetable(List<TimePeriod> timePeriods, SchoolDay[] week) {
            this.timePeriods = timePeriods;
            this.week = week;
        }

        public static async Task<LibrusTimetable> Retrieve(LibrusConnection connection, APIBufferMode bufferMode = APIBufferMode.none) { // TODO: this code is unmanageable. Please, rewrite it.
            string html = "";

            switch (bufferMode) {
                case APIBufferMode.none:
                    html = await Util.FetchAsync(requestUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    break;
                case APIBufferMode.load:
                    if (File.Exists("buffer_tt")) {
                        html = File.ReadAllText("buffer_tt");
                        break;
                    }
                    else
                        goto case APIBufferMode.save;
                case APIBufferMode.save:
                    html = await Util.FetchAsync(requestUrl, connection.cookieSession,
                        Util.SYNERGIA_INDEX);
                    File.WriteAllText("buffer_tt", html);
                    break;
            }
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var table = document.SelectSingleNode("/html/body/div[1]/div/div/div/form/table[2]");
            var rows = table.SelectNodes(".//tr").Skip(1).Reverse().Skip(1).Reverse(); // wtf

            List<TimePeriod> timePeriods = new List<TimePeriod>();
            List<Lesson>[] week = new List<Lesson>[7];

            List<SchoolDay> days = new List<SchoolDay>();

            int unknownCounter = 0; // counts unknown periods (breaks and other ones possibly)

            foreach (var item in rows) {
                var hrNode = item.SelectSingleNode("./*[2]");
                string hrText = Util.DeHtmlify(hrNode.InnerText);
                var idNode = item.SelectSingleNode("./*[1]");
                string idText = idNode.InnerText.Trim();

                string sHr = hrText.Split('-')[0].Trim();
                string eHr = hrText.Split('-')[1].Trim();

                var sDate = DateTime.Parse(sHr);
                var eDate = DateTime.Parse(eHr);

                if (idText == "") {
                    timePeriods.Add(new TimePeriod(sDate, eDate, -unknownCounter));
                    unknownCounter++;
                    continue;
                }

                timePeriods.Add(new TimePeriod(sDate, eDate, int.Parse(idText)));


                string snw = ""; // TODO: please, rewrite this nicely
                for (int i = 0; i < 7; i++) {
                    DateTime day = Util.GetFirstDayOfWeek(DateTime.Today, CultureInfo.InvariantCulture).AddDays(i);
                    
                    if (week[i] == null) week[i] = new List<Lesson>();
                    var nnn = item.SelectNodes("./*")[i + 2];
                    snw = nnn.InnerText;
                    snw = Util.DeHtmlify(snw);
                    bool rep = snw.ToLower().Contains("zastępstwo");
                    bool can = snw.ToLower().Contains("odwołane");
                    
                    var starthour = timePeriods.Last().start;
                    var endhour = timePeriods.Last().end;
                    if (snw == " " || snw == "") {
                        week[i].Add(new Lesson("", "", false, false, starthour, endhour, timePeriods.Last().mark));
                        continue;
                    }

                    string tc = snw.Substring(snw.IndexOf('-') + 1, snw.Length - snw.IndexOf('-') - 1);
                    
                    DateTime startDateTime = day.AddHours(starthour.Hour).AddMinutes(starthour.Minute);
                    DateTime endDateTime = day.AddHours(endhour.Hour).AddMinutes(endhour.Minute);
                    week[i].Add(new Lesson(
                        Util.DeHtmlify(nnn.SelectSingleNode(".//b").InnerText.Trim()).Replace("\n",""), 
                        tc.Trim(), rep, can, startDateTime, endDateTime,
                        timePeriods.Last().mark));
                }

            }

            int r = 0;
            foreach (var d in week) {
                r++;
                days.Add(new SchoolDay(d, Util.GetFirstDayOfWeek(DateTime.Now, CultureInfo.InvariantCulture).AddDays(r)) );
            }

            return new LibrusTimetable(timePeriods, days.ToArray());
        }
        
        
        
    }
}