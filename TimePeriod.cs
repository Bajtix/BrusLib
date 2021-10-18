using System;

namespace BrusLib {
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
}