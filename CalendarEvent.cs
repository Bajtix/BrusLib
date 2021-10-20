using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class CalendarEvent {
        public string Title { get; }
        public string Url { get; }
        
        public string Category { get; private set;}
        public string Description { get; private set;}
        
        public string TimePeriodText { get; private set;}

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

            foreach (var row in rows.Skip(1)) { // iterujemy przez wszystkie wiersze w tabeli i dzielimy ją na klucze i wartości
                if(!row.InnerHtml.Contains("th") || !row.InnerHtml.Contains("td")) continue;
                string key = row.SelectSingleNode(".//th").InnerText;
                string value = row.SelectSingleNode(".//td").InnerText;
                entries.Add(key, value);
            }
            Console.WriteLine("Sprawdzanie rzeczy");

            string day = "0000-01-01", lessonNo = "-1", timeFrame = "00:00 - 00:00"; // na podstawie danych próbujemy obliczyć przedział czasowy wydarzenia
            DateTime date = DateTime.MinValue, 
                startDate = DateTime.MinValue, 
                endDate = DateTime.MinValue;
            
            if (entries.TryGetValue("Data", out day)) {
                date = DateTime.Parse(day);
            }

            if (entries.TryGetValue("Nr lekcji", out lessonNo)) {
                int no = int.Parse(lessonNo);
                if (lessonPeriods != null) {
                    var period = lessonPeriods.First(w => w.mark == no);
                    startDate = period.start;
                    endDate = period.end;
                    startDate = DayHour(date, startDate);
                    endDate = DayHour(date, endDate);
                }
            }

            if (entries.TryGetValue("Przedział czasu", out timeFrame)) {
                string[] hours = timeFrame.Split('-');
                string s = Util.DeHtmlify(hours[0].Trim()); 
                string e = Util.DeHtmlify(hours[1].Trim());
                startDate = DateTime.Parse(s);
                endDate = DateTime.Parse(e); 
                Console.WriteLine("From hours");
                startDate = DayHour(date, startDate);
                endDate = DayHour(date, endDate);
            }
            
            if (entries.TryGetValue("Godziny", out timeFrame)) {
                string[] hours = timeFrame.Split('-');
                string s = Util.DeHtmlify(hours[0].Trim()); 
                string e = Util.DeHtmlify(hours[1].Trim());
                startDate = DateTime.Parse(s);
                endDate = DateTime.Parse(e); 
                Console.WriteLine("From hours");
                startDate = DayHour(date, startDate);
                endDate = DayHour(date, endDate);
            }

            string description, type;

            description = entries.TryGetValue("Opis", out description) ? description.Trim() : "???";
            type = entries.TryGetValue("Rodzaj", out type) ? type.Trim() : "???";
            

            StartsAt = startDate;
            EndsAt = endDate;

            TimePeriodText = $"{startDate:HH:mm} - {endDate:HH:mm}";

            Description = description;
            Category = type;

            // TODO: reszta właściwości
        }

        private static DateTime DayHour(DateTime d, DateTime h) {
            return new DateTime(d.Year, d.Month, d.Day, h.Hour, h.Minute, h.Second);
        }
    }
}