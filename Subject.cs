namespace BrusLib {
    public struct Subject {
        public string name;
        public Grade[] grades1;
        public Grade[] grades2;
        public Subject(string name, Grade[] grades1, Grade[] grades2) {
            this.name = name;
            this.grades1 = grades1;
            this.grades2 = grades2;
        }
        
    }
}