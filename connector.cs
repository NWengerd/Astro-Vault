using System;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;

class Database
{
    public static string connStr =
        "Server=localhost;Port=3306;Database=StarDB;User ID=root;Password=;";
} 


public class MySQL
{

// Return star object if successful, Return null if unsuccessful
public static star? queryStar(string attribute, string message)
{
    // Only allow recognized column names for the search attribute.
    string[] allowedAttributes =
    {
        "id", "proper_name", "hipID", "hdID", "hrID",
        "glID", "bayer", "varID", "flamsteed"
    };

    if (!allowedAttributes.Contains(attribute))
    {
        Console.WriteLine("Invalid search attribute.");
        return null;
    }

    try
    {
        using MySqlConnection conn = new MySqlConnection(Database.connStr);
        conn.Open();

        string queryS = $@"
            SELECT
                id,
                proper_name,
                hipID,
                hdID,
                hrID,
                glID,
                rightAsc,
                declination,
                dist,
                radialVel,
                absMag,
                spectral,
                bayer,
                flamsteed,
                constellation,
                luminosity,
                varID
            FROM Stars
            WHERE `{attribute}` = @message
            LIMIT 1;";

        using MySqlCommand cmd = new MySqlCommand(queryS, conn);
        cmd.Parameters.AddWithValue("@message", message);

        // Parse query output
        List<star> stars = ReadStarQueryOutput(cmd);

        // Return star if successful, null if unsuccessful
        return stars.Count > 0 ? stars[0] : null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database query failed: {ex.Message}");
        return null;
    }
}


// Return list of stars if successful, return null if unsuccessful
public static List<star>? queryStarList(int range)
{  
    try
    {
        using MySqlConnection conn = new MySqlConnection(Database.connStr);
        conn.Open();

        string queryS = $@"
            SELECT
                id,
                proper_name,
                hipID,
                hdID,
                hrID,
                glID,
                rightAsc,
                declination,
                dist,
                radialVel,
                absMag,
                spectral,
                bayer,
                flamsteed,
                constellation,
                luminosity,
                varID
            FROM Stars
            WHERE id >= @range
            AND id < @rangeEnd;";

        using MySqlCommand cmd = new MySqlCommand(queryS, conn);
        cmd.Parameters.AddWithValue("@range", range);
        cmd.Parameters.AddWithValue("@rangeEnd", range + 12);

        // Parse query output
        List<star> stars = ReadStarQueryOutput(cmd);

        // Return stars if successful, null if unsuccessful
        return stars.Count == 0 ? null : stars;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database query failed: {ex.Message}");
        return null;
    }  
}


// Take a MySQL command and return a list of stars found by that command
public static List<star> ReadStarQueryOutput(MySqlCommand cmd)
{
    using MySqlDataReader reader = cmd.ExecuteReader();

    List<star> stars = new List<star>();

    while (reader.Read())
    {
        // Convert database NULL values into C# nullable values.
        int? ReadInt(string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index)
                ? null
                : Convert.ToInt32(reader.GetValue(index));
        }

        double? ReadDouble(string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index)
                ? null
                : Convert.ToDouble(reader.GetValue(index));
        }

        string? ReadString(string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index)
                ? null
                : Convert.ToString(reader.GetValue(index));
        }

        star tuple = new star
        {
            id = ReadInt("id"),
            proper = ReadString("proper_name"),
            hip = ReadInt("hipID"),
            hd = ReadInt("hdID"),
            hr = ReadInt("hrID"),
            gl = ReadString("glID"),
            ra = ReadDouble("rightAsc"),
            dec = ReadDouble("declination"),
            dist = ReadDouble("dist"),
            rv = ReadDouble("radialVel"),
            absmag = ReadDouble("absMag"),
            spect = ReadString("spectral"),
            bayer = ReadString("bayer"),
            flam = ReadInt("flamsteed"),
            con = ReadString("constellation"),
            lum = ReadDouble("luminosity"),
            var = ReadString("varID")
        };

        stars.Add(tuple);
    }

    return stars;
}


// Return list of lines in a constellation if successful, return null if unsuccessful
public static constellationArt? queryConstellationArt(int constellation)
{
    try
    {
        using MySqlConnection conn = new MySqlConnection(Database.connStr);
        conn.Open();

        string queryS = $@"
            WITH StarsOfConst AS (SELECT id, rightAsc, declination FROM stars WHERE constellation = {constellation})
            SELECT stars1.rightasc AS rightAsc1, stars1.declination AS declination1, constellationArt.star1, 
            stars2.rightasc AS rightAsc2, stars2.declination AS declination2, constellationArt.star2 
                FROM ((SELECT star1, star2 FROM constellationArt WHERE constellation = {constellation}) as constellationArt 
                JOIN StarsOfConst AS stars1)
                JOIN StarsOfConst as stars2
            WHERE constellationArt.star1 = stars1.id 
            AND constellationArt.star2 = stars2.id;";

        using MySqlCommand cmd = new MySqlCommand(queryS, conn);

        // Parse query output
        constellationArt art = ReadArtQueryOutput(cmd);

        conn.Close();

        // Get full constellation name
        art.constFullName = GetFullConstellationName(constellation);

        // Return lines if successful, null if unsuccessful
        return art.lines?.Count == 0 ? null : art;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database query failed: {ex.Message}");
        return null;
    }        
}


// Return full constellation name if successful, return null if unsuccessful
public static string? GetFullConstellationName(int constellation)
{
    try
    {
        using MySqlConnection conn = new MySqlConnection(Database.connStr);
        conn.Open();

        string queryS = $@"
            SELECT fullName FROM constellations WHERE areaRank = {constellation} LIMIT 1;";

        using MySqlCommand cmd = new MySqlCommand(queryS, conn);

        using MySqlDataReader reader = cmd.ExecuteReader();

        // Read from query output and return full constellation name
        string? name = null;
        if (reader.Read())
        {
            int index = reader.GetOrdinal("fullName");
            name = reader.IsDBNull(index)
                ? null
                : Convert.ToString(reader.GetValue(index));
        }

        reader.Close();
        return name;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database query failed: {ex.Message}");
        return null;
    }        
}


// Take a MySQL command and return a list of lines found by that command
public static constellationArt ReadArtQueryOutput(MySqlCommand cmd)
{
    using MySqlDataReader reader = cmd.ExecuteReader();

    constellationArt art = new constellationArt
    {
        lines = new List<constellationLine?>()
    };

    while (reader.Read())
    {
        // Convert database NULL values into C# nullable values.
        int? ReadInt(string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index)
                ? null
                : Convert.ToInt32(reader.GetValue(index));
        }

        double? ReadDouble(string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index)
                ? null
                : Convert.ToDouble(reader.GetValue(index));
        }

        constellationLine tuple = new constellationLine
        {
            star1 = ReadInt("star1"),
            rightAsc1 = ReadDouble("rightAsc1"),
            declination1 = ReadDouble("declination1"),
            star2 = ReadInt("star2"),
            rightAsc2 = ReadDouble("rightAsc2"),
            declination2 = ReadDouble("declination2")
        };

        art.lines.Add(tuple);
    }

    reader.Close();

    return art;
}


public static void TestConnection()
{
    try
    {
        using var conn = new MySqlConnection(Database.connStr);
        conn.Open();
        using var dbCmd = new MySqlCommand("SELECT DATABASE();", conn);
        Console.WriteLine($"Connected database: {dbCmd.ExecuteScalar()}");

        Console.WriteLine("Successfully connected to MySQL!");

        using var cmd = new MySqlCommand(
            "SELECT DATABASE(), COUNT(*) FROM Stars;", conn);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            Console.WriteLine($"Database: {reader.GetString(0)}");
            Console.WriteLine($"Number of stars: {reader.GetInt64(1)}");
        }
    }
    catch (MySqlException ex)
    {
        Console.WriteLine($"MySQL connection failed: {ex.Message}");
    }
}
}