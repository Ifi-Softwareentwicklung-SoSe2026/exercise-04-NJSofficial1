namespace RoboterDatenverwaltung;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Landroboter), "landroboter")]
[JsonDerivedType(typeof(Lieferroboter), "lieferroboter")]
[JsonDerivedType(typeof(Schwimmroboter), "schwimmroboter")]
public abstract class Roboter : ISerializer
{
    public Roboter(string name, string typ, int energielevel, int maxGeschwindigkeit)
    {
        Name = name;
        Typ = typ;
        Energielevel = energielevel;
        MaxGeschwindigkeit = maxGeschwindigkeit;
    }
    public Roboter()
    {
        Name = "Unbekannt";
        Typ = "Unbekannt";
    }
    public string Name { get; set; }
    public string Typ { get; set; }
    public int Energielevel { get; set; }
    public int MaxGeschwindigkeit { get; set; }

    public void SpeichernAlsCSV(string dateipfad)
    {
        string inhalt = this is Lieferroboter lieferroboter
            ? $"{Name},{Typ},{Energielevel},{MaxGeschwindigkeit},{lieferroboter.AnzahlRaeder},{lieferroboter.Lieferkapazität}"
            : this is Landroboter landroboter
                ? $"{Name},{Typ},{Energielevel},{MaxGeschwindigkeit},{landroboter.AnzahlRaeder}"
                : this is Schwimmroboter schwimmroboter
                    ? $"{Name},{Typ},{Energielevel},{MaxGeschwindigkeit},{schwimmroboter.MaxTauchtiefe}"
                    : $"{Name},{Typ},{Energielevel},{MaxGeschwindigkeit}";
        File.WriteAllText(dateipfad, inhalt);
    }

    public static Roboter LadenAusCSV(string dateipfad)
    {
        string[] zeilen = File.ReadAllLines(dateipfad);
        string[] werte = zeilen[0].Split(',');

        string name = werte[0];
        string typ = werte[1];
        int energielevel = int.Parse(werte[2]);
        int maxGeschwindigkeit = int.Parse(werte[3]);

        if (typ == "Lieferroboter")
        {
            int anzahlRaeder = int.Parse(werte[4]);
            int lieferkapazitaet = int.Parse(werte[5]);
            return new Lieferroboter(name, energielevel, lieferkapazitaet, maxGeschwindigkeit)
            {
                AnzahlRaeder = anzahlRaeder
            };
        }

        if (typ == "Schwimmroboter")
        {
            int maxTauchtiefe = int.Parse(werte[4]);
            return new Schwimmroboter(name, energielevel, maxGeschwindigkeit, maxTauchtiefe);
        }

        if (typ == "Landroboter")
        {
            int anzahlRaeder = int.Parse(werte[4]);
            return new Landroboter(name, typ, energielevel, maxGeschwindigkeit)
            {
                AnzahlRaeder = anzahlRaeder
            };
        }

        return new Landroboter(name, typ, energielevel, maxGeschwindigkeit);
    }

    public void SpeichernAlsJSON(string dateipfad)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(dateipfad, json);
    }

    public static Roboter LadenAusJSON(string dateipfad)
    {
        string json = File.ReadAllText(dateipfad);
        Roboter? roboter = JsonSerializer.Deserialize<Roboter>(json) ?? throw new InvalidDataException($"JSON-Datei konnte nicht gelesen werden: {dateipfad}");
        return roboter;
    }

    public virtual string GetStatus()
    {
        return $"Roboter - Name: {Name}, Typ: {Typ}, Energielevel: {Energielevel}";
    }

    public virtual void Activate()
    {
        if (Energielevel > 0)
        {
            Console.WriteLine("activated");
            Energielevel--;
            return;
        }

        Console.WriteLine("energy depleted");
    }
}

public class Landroboter : Roboter
{
    public int AnzahlRaeder { get; set; }
    public Landroboter() : base()
    {
    }
    public Landroboter(string name, string typ, int energielevel, int maxGeschwindigkeit) : base(name, typ, energielevel, maxGeschwindigkeit)
    {
    }
}

public class Lieferroboter : Landroboter
{
    public int Lieferkapazität { get; set; }
    public Lieferroboter() : base()
    {
        Typ = "Lieferroboter";
    }
    public Lieferroboter(string name, int energielevel, int lieferkapazität, int maxGeschwindigkeit) : base(name, "Lieferroboter", energielevel, maxGeschwindigkeit)
    {
        Lieferkapazität = lieferkapazität;
    }

    public override string GetStatus()
    {
        return $"Lieferroboter - Name: {Name}, Typ: {Typ}, Energielevel: {Energielevel}, Lieferkapazität: {Lieferkapazität}";
    }
}

public class Schwimmroboter : Roboter
{
    public int MaxTauchtiefe { get; set; }
    public Schwimmroboter() : base()
    {
        Typ = "Schwimmroboter";
    }

    public Schwimmroboter(string name, int energielevel, int maxGeschwindigkeit, int maxTauchtiefe) : base(name, "Schwimmroboter", energielevel, maxGeschwindigkeit)
    {
        MaxTauchtiefe = maxTauchtiefe;
    }
}