using System;
using System.Collections.Generic;

namespace BrusLib {
    public struct SchoolDay {
        public List<Lesson> lessons  { get; }
        public DateTime day { get; }
        
        public SchoolDay(List<Lesson> lessons, DateTime day) {
            this.lessons = lessons;
            this.day = day;
        }
    }
}