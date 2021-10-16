using System;
using System.Collections.Generic;

namespace BrusLib {
    public class SchoolEvent {
        public int lessonNo;
        public string content;
        public DateTime day;
        
        public SchoolEvent(string content, DateTime day) {
            this.content = content;
            this.day = day;
            
            var s = TryParseThatShit(content);
            if(s.ContainsKey("Nr lekcji"))
                lessonNo = int.Parse(s["Nr lekcji"]);
            else
                lessonNo = -1;
        }

        protected Dictionary<string,string> TryParseThatShit(string shit) {
            Dictionary<string,string> result = new Dictionary<string,string>();
            string unsortedData = "";

            shit = shit.Replace("&lt;br /&gt;", "<br>");
            string[] lines = shit.Split(new string[] {"<br>", "br /&gt"}, StringSplitOptions.None);

            foreach (string line in lines) {
                if (line.Contains(':') || line.Contains('>')) {
                    string key = line.Split(':','>')[0].Trim();
                    string value = line.Split(':', '>')[1].Trim();
                    result.Add(key,value);
                }
                else {
                    unsortedData += line + ";";
                }
                
            }

            unsortedData = unsortedData.Substring(0, unsortedData.Length - 1);

            result.Add("other",unsortedData);

            return result;
        }
    }
}