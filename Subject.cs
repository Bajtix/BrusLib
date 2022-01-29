namespace BrusLib {
    public struct Subject {
        public string name { get; set; }
        public Grade[] grades1 { get; }
        public Grade[] grades2 { get; }
        public Subject(string name, Grade[] grades1, Grade[] grades2) {
            this.name = name;
            this.grades1 = grades1;
            this.grades2 = grades2;
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return ((Subject)obj).name == this.name;
        }
        
        public static bool operator == (Subject a, Subject b) {
            return a.Equals(b);
        }
        
        public static bool operator != (Subject a, Subject b) {
            return !(a == b);
        }
    }
}