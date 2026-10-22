using System.Globalization;
using CsvHelper;

public class Foo
{
    public int? id { get; set; }
    public int? hip { get; set; }
    public int? hd { get; set; }
    public int? hr { get; set; }
    public string? gl { get; set; }
    // public string bf { get; set; }
    public string? proper { get; set; }
    public double? ra { get; set; }
    public double? dec { get; set; }
    public double? dist { get; set; }
    public double? rv { get; set; }
    public double? absmag { get; set; }
    public string? spect { get; set; }
    public string? bayer { get; set; }
    public int? flam { get; set; }
    public string? con { get; set; }
    public double? lum { get; set; }
    public string? var { get; set; }
}

public class CSVParser
{
    public static void Main()
    {
        string path = @".\hyg_v42.csv";

        // Open file in temporary scope
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // Read file contents sequentially
            var records = csv.GetRecords<Foo>(); // Fills records using Foo's member names and order
            foreach (var record in records)
            {
                Console.WriteLine($"ID: {record.id}, proper_name: {record.proper}, constellation: {record.con}");
            }
        }
    }
}