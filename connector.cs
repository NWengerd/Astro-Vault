using System;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;

public class MySQL
{
    public static void test()
    {
        // Note: This connection string is temporary and needs to match the db server on the docker container
        string connStr = "server=localhost;user=root;database=sys;port=3306;password=av985";
        // Create connection to database specified in connStr
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            Console.WriteLine("Connecting to MySQL...");
            conn.Open();
            
            // Create query for Stars table
            string queryS = "SELECT * FROM stars;";
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

    // Return first tuple if successful, Return null if unsuccessful
    public static star? queryStarsDB(String attribute, String message)
    {
        // Note: This connection string is temporary and needs to match the db server on the docker container
        string connStr = "server=localhost;user=root;database=db_name;port=3306;password=password";
        // Create connection to database specified in connStr
        MySqlConnection conn = new MySqlConnection(connStr);

        star? tuple = null;

        try
        {
            Console.WriteLine("Connecting to MySQL...");
            conn.Open();
            
            // Construct query for Stars table using arguments
            string queryS = "SELECT * FROM stars WHERE " + attribute + " = \"" + message + "\";";
            MySqlCommand cmd = new MySqlCommand(queryS, conn);
            // Execute query
            MySqlDataReader reader = cmd.ExecuteReader();

            // Read first tuple returned, if there is one to read
            if(reader.Read()) {
                tuple = new star(reader.GetInt32(0), reader.GetString(1));
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
        return tuple;
    }
}