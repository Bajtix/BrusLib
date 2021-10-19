using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class CalendarEvent {
        public string Title { get; }
        public string Url { get; }

        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }

        public CalendarEvent(string title, string url) {
            Title = title;
            Url = url;
        }

        public async Task FetchInfo(LibrusConnection connection, List<TimePeriod> lessonPeriods = null) {
            string html = await Util.FetchAsync(Url, connection.cookieSession, "https://synergia.librus.pl/terminarz");
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;

            var table = document.SelectSingleNode("/html/body/div[3]/div[3]/form/div/div/table");
            var rows = table.SelectNodes(".//tr");

            Dictionary<string, string> entries = new Dictionary<string, string>();

            foreach (var row in rows) { // iterujemy przez wszystkie wiersze w tabeli i dzielimy ją na klucze i wartości
                string key = row.SelectSingleNode("./th").InnerText;
                string value = row.SelectSingleNode("./td").InnerText;
                entries.Add(key, value);
            }

            string day = "0000-01-01", lessonNo = "-1", timeFrame = "00:00 - 00:00"; // na podstawie danych próbujemy obliczyć przedział czasowy wydarzenia
            DateTime date, startDate, endDate;
            if (entries.TryGetValue("Data", out day)) {
                date = DateTime.Parse(day);
            }

            if (entries.TryGetValue("Nr lekcji", out lessonNo)) {
                int no = int.Parse(lessonNo);
                if (lessonPeriods != null) {
                    var period = lessonPeriods.First(w => w.mark == no);
                    startDate = period.start;
                    endDate = period.end; // TODO: połącz dni z date z godzinami z tąd
                }
            }

            if (entries.TryGetValue("Przedział czasu", out timeFrame)) {
                string[] hours = timeFrame.Split('-');
                string s = hours[0].Trim(); 
                string e = hours[1].Trim();
                startDate = DateTime.Parse(s);
                endDate = DateTime.Parse(e); // TODO: połącz dni z date z godzinami z tąd
            }
            
            // TODO: reszta właściwości
        }
    }
}