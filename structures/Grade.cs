using System;
using System.Drawing;

namespace BrusLib2;

public class Grade {
    public string category, comment;
    public string value;
    public Subject? subject;
    public DateTime addedDate;
    public byte weight;
    public bool accountForInAverage;
    public Color color;

    public Grade(string value, Subject subject) {
        this.value = value;
        this.subject = subject;
        category = comment = string.Empty;
        weight = 0;
        addedDate = DateTime.UnixEpoch;
        accountForInAverage = false;
        color = Color.Purple;
    }

    public Grade(string category, string comment, string value, DateTime addedDate, byte weight, Color color, bool accountForInAverage, Subject? subject = null) {
        this.category = category;
        this.comment = comment;
        this.value = value;
        this.subject = subject;
        this.addedDate = addedDate;
        this.weight = weight;
        this.color = color;
    }
}