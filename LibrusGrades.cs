﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrusLib {
    public class LibrusGrades {
        public readonly List<Subject> subjects;
        public readonly DateTime lastFetched;

        public static async Task<LibrusGrades> Retrieve(LibrusConnection connection) {
            string html = await Util.FetchAsync("https://synergia.librus.pl/przegladaj_oceny/uczen", connection.cookieSession,
                Util.SYNERGIA_INDEX);
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var document = doc.DocumentNode;
            // this segment gets your grades
            var lines = document.SelectNodes("/html/body/div[3]/div[3]/form[1]/div/div/table/tr"); // get grade table
            List<Subject> subjects = lines
                .Where((c, i) => i % 2 == 0) // select every other element - odd ones are just dumb copies
                .Select(line => 
                    new Subject(
                        line.SelectSingleNode("./td[2]").InnerText.Trim(), 
                        GetGrades(line.SelectSingleNode("./td[3]")), 
                        GetGrades(line.SelectSingleNode("./td[4]"))))
                .ToList();
            
            return new LibrusGrades(subjects, DateTime.Now);
        }
        
        protected LibrusGrades(List<Subject> subjects, DateTime lastFetched) {
            this.subjects = subjects;
            this.lastFetched = lastFetched;
        }
        
        private static Grade[] GetGrades(HtmlNode tableRow) {
            var grades = tableRow.SelectNodes(".//a[@class=\"ocena\"]");
            if(grades == null || grades.Count == 0) return Array.Empty<Grade>();

            List<Grade> gs = new List<Grade>();
            foreach (var item in grades) {
                if(item.InnerText.Trim() == String.Empty) continue;
                gs.Add(new Grade(
                    item.InnerText, 
                    ColorFromStyle(item.ParentNode.GetAttributeValue("style", "background-color: rgb(255,0,255);")), 
                    item.GetAttributeValue("title","Error Fetching")));
            }

            return gs.ToArray();
        }
        
        private static Color ColorFromStyle(string style) {
            /*style = style.Substring(style.IndexOf("(") + 1, style.IndexOf(")") - style.IndexOf("(") - 1);
            
            var w = style.Split(',');
            
            return Color.FromArgb(int.Parse(w[0]),int.Parse(w[1]),int.Parse(w[2])); // TODO: implement*/

            return ColorFromHex(style.Split(':')[1].Replace(";","").Trim());
        }

        private static Color ColorFromHex(string hex) {
            hex = hex.Substring(1);
            string rs = hex.Substring(0, 2);
            string gs = hex.Substring(2, 2);
            string bs = hex.Substring(4, 2);

            int r = int.Parse(rs, NumberStyles.HexNumber);
            int g = int.Parse(gs, NumberStyles.HexNumber);
            int b = int.Parse(bs, NumberStyles.HexNumber);

            return Color.FromArgb(r, g, b);
        }
    }
}