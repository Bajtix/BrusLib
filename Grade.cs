using System;
using System.Drawing;

namespace BrusLib {
    public struct Grade {
        public string grade { get; set;}
        public Color color { get; set;}
        public string category { get; set;}
        public string comment { get; set;  }
        public DateTime addDate { get; set;  }
        public bool accountsIntoFinal { get; set;  }
        
        public Grade(string grade, Color color, string meta) {
            this.grade = grade;
            this.color = color;
            var d = ParseMeta(meta);
            category = d.cat;
            comment = d.com;
            addDate = d.dat;
            accountsIntoFinal = d.cnt;
        }

        private static (string cat, string com, DateTime dat, bool cnt) ParseMeta(string meta) {
            string cat = "", com = "No comment";
            DateTime dat = DateTime.MinValue;
            bool cnt = false;
            meta = meta.Replace("<br>", "\n");
            string[] lines = meta.Split('\n');
            foreach (var item in lines) {
                string key = item.Split(':')[0].ToLower().Trim();
                string val = item.Split(':')[1].Trim();
                switch (key) {
                    case "kategoria":
                        cat = val;
                        break;
                    case "data":
                        dat = ParseDate(val);
                        break;
                    case "komentarz":
                        com = val;
                        break;
                    case "licz do średniej":
                        cnt = val.ToLower().Trim() == "tak";
                        break;
                }
            }

            return (cat, com, dat, cnt);
        }

        private static DateTime ParseDate(string v) {
            return DateTime.Today;
        }

        public override string ToString() {
            return $"{grade} : [{category}] : {comment}";
        }
    }
}