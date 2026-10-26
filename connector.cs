using System;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;

public class MySQL
{
    public static void test()
    {
        // Note: This connection string is temporary and needs to match the db server on the docker container
        string connStr = "Server=localhost;Port=3306;Database=StarDB;User ID=root;Password=;";
        // Create connection to database specified in connStr
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            Console.WriteLine("Connecting to MySQL...");
            conn.Open();
            
            // Create query for Stars table
            string queryS = "SELECT * FROM Stars;";
            MySqlCommand cmd = new MySqlCommand(queryS, conn);
            
            // Execute query
            MySqlDataReader reader = cmd.ExecuteReader();

            // Read data from executed query
            while(reader.Read())
            {
                Console.WriteLine(reader[1]+"\n");
            }
            // Close query so more queries can be made
            reader.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        
        // Close connection
        conn.Close();
        Console.WriteLine("Done.");
    }

    // Return star object if successful, Return null if unsuccessful
    public static star? queryStarsDB(string attribute, string message)
{
    string connStr =
        "Server=localhost;Port=3306;Database=StarDB;User ID=root;Password=;";

    star? tuple = null;

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

        using MySqlDataReader reader = cmd.ExecuteReader();

        if (reader.Read())
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

            tuple = new star
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
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database query failed: {ex.Message}");
    }

    return tuple;
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