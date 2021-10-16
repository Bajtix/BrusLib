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
        
    }
}