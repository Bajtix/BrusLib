using System;

namespace BrusLib {
    public struct Lesson {
        public string name { get; }
        public string teacher { get;}
        public DateTime from { get; }
        public DateTime to { get; }
        public bool isReplacement { get; }
        public bool isCancelled { get; }
        public int lessonNum { get; }
            
        public Lesson(string name, string teacher, bool isReplacement, bool isCancelled, DateTime from, DateTime to, int lessonNum) {
            this.name = name;
            this.teacher = teacher;
            this.isReplacement = isReplacement;
            this.from = from;
            this.to = to;
            this.isCancelled = isCancelled;
            this.lessonNum = lessonNum;
        }
    }
}