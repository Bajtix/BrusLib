using System;
using HtmlAgilityPack;

namespace BrusLib {
    public class SEExam : SchoolEvent {
        public string description;
        public string subject;
        
        public SEExam(string content, DateTime day) : base(content, day) {
            var s = TryParseThatShit(content);
            if(s.ContainsKey("Opis"))
                description = s["Opis"];
            if (s.ContainsKey("<span class=\"przedmiot\""))
                subject = s["<span class=\"przedmiot\""].Split('<')[0];

        }
    }
}