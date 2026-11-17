using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MySql.Data;
using MySql.Data.MySqlClient;

public class constellationArt
{
    public required List<constellationLines> constellations { get; set; }
}

public class constellationLines
{
    public required string iau { get; set; }
    public required List<List<int>> lines { get; set; }
}


public class artReader
{
    public static void ReadConstellationArt()
    {   
        // stringify json
        string json = File.ReadAllText("ruelleV3.json");
        
        // convert json into c# object
        try
        {
            constellationArt art = JsonSerializer.Deserialize<constellationArt>(json) 
                ?? throw new Exception("Failed to parse ruelleV3.json");

            InsertConstellationArt(art);
        }
        catch (Exception er)
        {
            Console.WriteLine(er.Message);
        }
    }


    private static void InsertConstellationArt(constellationArt art)
    {
        Dictionary<string, int> constellationIds = GetConstellationIds(art);

        // Get star IDs for each star in constellationArt
        for (int i = 0; i < art.constellations.Count; i++)
        {
            for (int j = 0; j < art.constellations[i].lines.Count; j++)
            {
                for (int k = 0; k < art.constellations[i].lines[j].Count; k++)
                {
                    // Replace existing star name with its ID in the database
                    art.constellations[i].lines[j][k] = 
                        GetStarId(art.constellations[i].lines[j][k]);
                }
            }
        }

        // Initialize ID counter for constellationArt table
        int idCounter = 1;

        // Insert constellation lines into the database
        for (int i = 0; i < art.constellations.Count; i++)
        {
            for (int j = 0; j < art.constellations[i].lines.Count; j++)
            {
                // Skip last star in line
                for (int k = 0; k < art.constellations[i].lines[j].Count - 1; k++)
                {
                    // Replace existing star name with its ID in the database
                    InsertConstellationLine(
                        idCounter++,                                    // id
                        art.constellations[i].lines[j][k],              // Star 1
                        art.constellations[i].lines[j][k + 1],          // Star 2
                        constellationIds[art.constellations[i].iau]);   // Constellation ID                    
                }
            }
        }

        Console.WriteLine("Constellation art inserted successfully!");
    }


    private static int GetStarId(int star)
    {
        try
        {
            using MySqlConnection conn = new MySqlConnection(Database.connStr);
            conn.Open();

            string queryS = $@"
                SELECT
                    id
                FROM Stars
                WHERE hipid = @starName
                LIMIT 1;";

            using MySqlCommand cmd = new MySqlCommand(queryS, conn);
            cmd.Parameters.AddWithValue("@starName", star);

            using MySqlDataReader reader = cmd.ExecuteReader();

            // Return queried star id
            if(reader.Read())
            {   
                int starId = Convert.ToInt32(reader["id"]);
                
                reader.Close();
                conn.Close();

                return starId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while fetching star ID: {ex.Message}");
        }

        return 0;
    }


    // Return a dictionary mapping constellation names to their areaRank IDs
    private static Dictionary<string, int> GetConstellationIds(constellationArt art)
    {
        Dictionary<string, int> conIds = new Dictionary<string, int>();
        
        try
        {
            using MySqlConnection conn = new MySqlConnection(Database.connStr);
            conn.Open();

            string queryS = $@"
                SELECT
                    areaRank,
                    name
                FROM Constellations;";

            using MySqlCommand cmd = new MySqlCommand(queryS, conn);
            using MySqlDataReader reader = cmd.ExecuteReader();

            // Read queried tuples into dictionary
            while(reader.Read())
            {
                conIds[reader["name"].ToString() ?? "---"] = Convert.ToInt32(reader["areaRank"]);
            }

            reader.Close();
            conn.Close();

            return conIds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while fetching constellation IDs: {ex.Message}");
            return conIds;
        }
    }


    private static void InsertConstellationLine(int id, int star1, int star2, int con)
    {
        try
        {
            using MySqlConnection conn = new MySqlConnection(Database.connStr);
            conn.Open();

            string insert = $@"
                INSERT INTO constellationArt (id, star1, star2, constellation)
                VALUES ({id}, {star1}, {star2}, {con});";

            MySqlCommand cmd = new MySqlCommand(insert, conn);
            cmd.ExecuteNonQuery();

            conn.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while inserting constellation line: {ex.Message}");
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



