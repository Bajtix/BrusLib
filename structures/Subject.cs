namespace BrusLib2;

public class Subject {
    public string name = string.Empty;
    public Grade[] grades = new Grade[0];

    public Subject(string name) {
        this.name = name;
    }

    public static implicit operator Grade[](Subject s) => s.grades;

    public float CalculateAverage() {
        float sum = 0;
        float totalWeight = 0;
        foreach (Grade grade in grades) {
            float? nm = grade.GetNumerical();
            if (nm == null || !grade.accountForInAverage) continue;
            sum += nm.Value * grade.weight;
            totalWeight += grade.weight;
        }

        return sum / totalWeight;
    }
}