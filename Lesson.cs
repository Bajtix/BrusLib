using System;

namespace BrusLib {
    public struct Lesson {
        public string name { get; }
        public string AdditionalInfo { get;}
        public string Teacher { get;}
        public string Classroom { get;}
        public DateTime from { get; }
        public DateTime to { get; }
        public bool isReplacement { get; }
        public bool isCancelled { get; }
        public int lessonNum { get; }
            
        public Lesson(string name, string additionalInfo, bool isReplacement, bool isCancelled, DateTime from, DateTime to, int lessonNum) {
            this.name = name;
            this.AdditionalInfo = additionalInfo;
            this.isReplacement = isReplacement;
            this.from = from;
            this.to = to;
            this.isCancelled = isCancelled;
            this.lessonNum = lessonNum;
            this.Teacher = "???";
            this.Classroom = "???";

            int sDotIndex = additionalInfo.Contains("s.") ? additionalInfo.IndexOf("s.", StringComparison.OrdinalIgnoreCase) : 9999;
            int braceIndex = additionalInfo.Contains("(") ? additionalInfo.IndexOf("(", StringComparison.OrdinalIgnoreCase) : 9999;
            int trimIndex = Math.Min(sDotIndex, braceIndex);
            if (trimIndex < 9999) {
                Teacher = additionalInfo.Substring(0, trimIndex);
                
            }

            if (sDotIndex < 9999) {
                Classroom = additionalInfo.Substring(sDotIndex);
            }
        }
        
    }
}