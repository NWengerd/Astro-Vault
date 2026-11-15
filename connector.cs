using System;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;

public class MySQL
{

// Return star object if successful, Return null if unsuccessful
public static star? queryStar(string attribute, string message)
{
    string connStr =
        "Server=localhost;Port=3306;Database=StarDB;User ID=root;Password=;";

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
        using MySqlConnection conn = new MySqlConnection(connStr);
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
    string connStr =
        "Server=localhost;Port=3306;Database=StarDB;User ID=root;Password=;";
    
    try
    {
        using MySqlConnection conn = new MySqlConnection(connStr);
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


public static void TestConnection()
{
    string connStr =
        "Server=localhost;Port=3306;Database=StarDB;User ID=root;Password=;";

    try
    {
        using var conn = new MySqlConnection(connStr);
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