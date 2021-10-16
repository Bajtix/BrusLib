using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusTimetable {

        public struct TimePeriod {
            public DateTime start, end;
            public int mark;
            public TimePeriod(DateTime start, DateTime end, int mark) {
                this.start = start;
                this.end = end;
                this.mark = mark;
            }

            public override string ToString() {
                return $"{(mark>=0?"Lekcja ":"Wolna ")} {Math.Abs(mark)} === {start:hh:mm:ss} - {end:hh:mm:ss}";
            }
        }
        
        public struct Lesson {
            public string name;
            public string teacher;
            public DateTime from, to;
            public bool isReplacement;
            public int lessonNum;
            
            public Lesson(string name, string teacher, bool isReplacement, DateTime from, DateTime to, int lessonNum) {
                this.name = name;
                this.teacher = teacher;
                this.isReplacement = isReplacement;
                this.from = from;
                this.to = to;
                this.lessonNum = lessonNum;
            }
        }

        public List<TimePeriod> timePeriods;

        public List<Lesson>[] week;


        public LibrusTimetable(List<TimePeriod> timePeriods, List<Lesson>[] week) {
            this.timePeriods = timePeriods;
            this.week = week;
        }

        public static async Task<LibrusTimetable> Retrieve(LibrusConnection connection) {
            string html/* = await Util.FetchAsync("https://synergia.librus.pl/przegladaj_plan_lekcji", connection.cookieSession,
                Util.SYNERGIA_INDEX)*/;
            
            //File.WriteAllText("plan req.html",html);
            html = File.ReadAllText("plan req.html");
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var table = document.SelectSingleNode("/html/body/div[1]/div/div/div/form/table[2]");
            var rows = table.SelectNodes(".//tr").Skip(1).Reverse().Skip(1).Reverse(); // wtf

            List<TimePeriod> timePeriods = new List<TimePeriod>();
            List<Lesson>[] week = new List<Lesson>[7];

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
                    DateTime day = GetFirstDayOfWeek(DateTime.Today, CultureInfo.CurrentCulture).AddDays(i);
                    if (week[i] == null) week[i] = new List<Lesson>();
                    var nnn = item.SelectNodes("./*")[i + 2];
                    snw = nnn.InnerText;
                    //snw = snw.Replace("\n","").Replace("<br>", "\n").Trim();
                    snw = Util.DeHtmlify(snw);
                    if (snw == " " || snw == "") {
                        week[i].Add(new Lesson());
                        continue;
                    }

                    string tc = snw.Substring(snw.IndexOf('-') + 1, snw.Length - snw.IndexOf('-') - 1);
                    var starthour = timePeriods.Last().start;
                    var endhour = timePeriods.Last().end;
                    DateTime start_exact = day.AddHours(starthour.Hour).AddMinutes(starthour.Minute);
                    DateTime end_exact = day.AddHours(endhour.Hour).AddMinutes(endhour.Minute);
                    week[i].Add(new Lesson(nnn.SelectSingleNode(".//b").InnerText, tc, false, start_exact, end_exact,
                        timePeriods.Last().mark));
                }

            }

            return new LibrusTimetable(timePeriods, week);
        }
        
        
        private static DateTime GetFirstDayOfWeek(DateTime dayInWeek, CultureInfo cultureInfo) {
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dayInWeek.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }
    }
}