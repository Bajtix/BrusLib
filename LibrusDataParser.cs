namespace BrusLib2;

using System;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;

public static class LibrusDataParser {
    public static Subject[] ParseHTMLSubjects(string html) { // TODO: refactor this, so it has a similar structure to ParseHTMLGrade
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/form[1]/div/div/table");
        var rows = table.SelectNodes("./tr"); // maybe get only the even rows? add .Where((c, i) => i % 2 == 0)

        var subjects = new List<Subject>();

        foreach (var row in rows) {
            var cells = row.SelectNodes("td");
            if (cells.Count < 9) continue; // skip if not enough columns
            string subjectName = cells[1].InnerText;
            if (subjects.Any(s => s.name == subjectName)) continue; // skip subject if exists
            var subject = new Subject(subjectName);
            var gradeNodes = cells[2].SelectNodes("./span");
            if (gradeNodes == null) continue; // if we have no grades, ignore the subject

            var gradesList = new List<Grade>();
            foreach (var gradeNode in gradeNodes) if (ParseHTMLGrade(subject, gradeNode) is Grade grade) gradesList.Add(grade!); //ellegant. Thanks, copilot.
            subject.grades = gradesList.ToArray();

            subjects.Add(subject);
        }


        return subjects.ToArray();
    }

    public static Grade? ParseHTMLGrade(Subject sub, HtmlNode html) {
        var infoNode = html.SelectSingleNode("./a");
        if (infoNode == null) return null;

        var info = infoNode.GetAttributeValue("title", "null").HTMLReformatLineBreaks().Split('\n');
        var grade = new Grade(html.InnerText.HTMLRemoveTags().Trim(), sub);

        foreach (var infoPart in info) {
            if (!infoPart.Contains(":")) continue;
            var key = infoPart.Split(':')[0].Trim();
            var val = infoPart.Split(':')[1].Trim();

            switch (key) {
                case "Ocena":
                    grade.value = val;
                    break;
                case "Kategoria":
                    grade.category = val.HTMLRemoveTags();
                    break;
                case "Komentarz":
                    grade.comment = val.HTMLRemoveTags();
                    break;
                case "Data":
                    val = val.Split('(')[0].Trim();
                    grade.addedDate = DateTime.Parse(val.HTMLRemoveTags(), System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Waga":
                    grade.weight = byte.Parse(val.HTMLRemoveTags());
                    break;
                case "Licz do średniej":
                    grade.accountForInAverage = val.HTMLRemoveTags().ToLower() == "tak";
                    break;
            }

            //TODO: set color of grade when loading it
        }
        return grade;
    }
}