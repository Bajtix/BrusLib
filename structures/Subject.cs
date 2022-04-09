namespace BrusLib2;

public class Subject {
    public string name = string.Empty;
    public Grade[] grades = new Grade[0];

    public Subject(string name) {
        this.name = name;
    }

    public static implicit operator Grade[](Subject s) => s.grades;

}