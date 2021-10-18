using System;
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


        public LibrusTimetable(List<TimePeriod> timePeriods, SchoolDay[] week) {
            this.timePeriods = timePeriods;
            this.week = week;
        }

        public static async Task<LibrusTimetable> Retrieve(LibrusConnection connection) {
            string html = await Util.FetchAsync("https://synergia.librus.pl/przegladaj_plan_lekcji", connection.cookieSession,
                Util.SYNERGIA_INDEX);
            
            //File.WriteAllText("plan req.html",html);
            //html = File.ReadAllText("plan req.html");
            
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
                var hr_node = item.SelectSingleNode("./*[2]");
                string hr_text = Util.DeHtmlify(hr_node.InnerText);
                var id_node = item.SelectSingleNode("./*[1]");
                string id_text = id_node.InnerText.Trim();

                string s_hr = hr_text.Split('-')[0].Trim();
                string e_hr = hr_text.Split('-')[1].Trim();

                var s_date = DateTime.Parse(s_hr);
                var e_date = DateTime.Parse(e_hr);

                if (id_text == "") {
                    timePeriods.Add(new TimePeriod(s_date, e_date, -unknownCounter));
                    unknownCounter++;
                    continue;
                }

                timePeriods.Add(new TimePeriod(s_date, e_date, int.Parse(id_text)));


                string snw = "";
                for (int i = 0; i < 7; i++) {
                    DateTime day = Util.GetFirstDayOfWeek(DateTime.Today, CultureInfo.InvariantCulture).AddDays(i);
                    if (week[i] == null) week[i] = new List<Lesson>();
                    var nnn = item.SelectNodes("./*")[i + 2];
                    snw = nnn.InnerText;
                    snw = Util.DeHtmlify(snw);
                    bool rep = snw.ToLower().Contains("zastępstwo");
                    bool can = snw.ToLower().Contains("odwołane");
                    //snw = snw.Replace("\n","").Replace("<br>", "\n").Trim();
                    var starthour = timePeriods.Last().start;
                    var endhour = timePeriods.Last().end;
                    if (snw == " " || snw == "") {
                        week[i].Add(new Lesson("", "", false, false, starthour, endhour, timePeriods.Last().mark));
                        continue;
                    }

                    string tc = snw.Substring(snw.IndexOf('-') + 1, snw.Length - snw.IndexOf('-') - 1);
                    
                    DateTime start_exact = day.AddHours(starthour.Hour).AddMinutes(starthour.Minute);
                    DateTime end_exact = day.AddHours(endhour.Hour).AddMinutes(endhour.Minute);
                    week[i].Add(new Lesson(Util.DeHtmlify(nnn.SelectSingleNode(".//b").InnerText.Trim()).Replace("\n",""), tc, rep, can, start_exact, end_exact,
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