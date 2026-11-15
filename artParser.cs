using System.Text.Json;
using System.Text.Json.Serialization;

public class constellationArt
{
    public required List<constellationLines> constellations { get; set; }
}

public class constellationLines
{
    public string? IAU { get; set; }
    public required List<List<string>> lines { get; set; }
    public List<string>? variant_of { get; set; }
}


public class artReader
{
    public static void ReadConstellationArt()
    {   
        // stringify json
        string json = File.ReadAllText("Ruelle_simplified.json");
        
        // convert json into c# object
        try
        {
            constellationArt art = JsonSerializer.Deserialize<constellationArt>(json) 
                ?? throw new Exception("Failed to parse Ruelle_simplified.json");

            // Dump contents for testing
            // WriteConstellationArtAsync(art, "test.json");
        }
        catch (Exception er)
        {
            Console.WriteLine(er.Message);
        }
    }

    // Options for writing JSON files with indentation and ignoring null values
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    // Write constellation art to a JSON file (for testing purposes)
    public static void WriteConstellationArtAsync(constellationArt art, string filePath)
    {
        using FileStream stream = File.Create(filePath);
        JsonSerializer.Serialize(stream, art, WriteOptions);
    }
}



