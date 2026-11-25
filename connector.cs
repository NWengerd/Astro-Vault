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
        star tuple = new star
        {
            id = ReadInt("id", reader),
            proper = ReadString("proper_name", reader),
            hip = ReadInt("hipID", reader),
            hd = ReadInt("hdID", reader),
            hr = ReadInt("hrID", reader),
            gl = ReadString("glID", reader),
            ra = ReadDouble("rightAsc", reader),
            dec = ReadDouble("declination", reader),
            dist = ReadDouble("dist", reader),
            rv = ReadDouble("radialVel", reader),
            absmag = ReadDouble("absMag", reader),
            spect = ReadString("spectral", reader),
            bayer = ReadString("bayer", reader),
            flam = ReadInt("flamsteed", reader),
            con = GetFullConstellationName(ReadInt("constellation", reader)),
            lum = ReadDouble("luminosity", reader),
            var = ReadString("varID", reader)
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
public static string? GetFullConstellationName(int? constellation)
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
        constellationLine tuple = new constellationLine
        {
            star1 = ReadInt("star1", reader),
            rightAsc1 = ReadDouble("rightAsc1", reader),
            declination1 = ReadDouble("declination1", reader),
            star2 = ReadInt("star2", reader),
            rightAsc2 = ReadDouble("rightAsc2", reader),
            declination2 = ReadDouble("declination2", reader)
        };

        art.lines.Add(tuple);
    }

    reader.Close();

    return art;
}


// Pull int from SQL query reader
private static int? ReadInt(string column, MySqlDataReader reader)
{
    int index = reader.GetOrdinal(column);
    return reader.IsDBNull(index)
        ? null
        : Convert.ToInt32(reader.GetValue(index));
}

// Pull double from SQL query reader
private static double? ReadDouble(string column, MySqlDataReader reader)
{
    int index = reader.GetOrdinal(column);
    return reader.IsDBNull(index)
        ? null
        : Convert.ToDouble(reader.GetValue(index));
}

// Pull string from SQL query reader
private static string? ReadString(string column, MySqlDataReader reader)
{
    int index = reader.GetOrdinal(column);
    return reader.IsDBNull(index)
        ? null
        : Convert.ToString(reader.GetValue(index));
}
}