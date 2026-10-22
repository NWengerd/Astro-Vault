using System;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;

public class MySQL
{
    public static void Main()
    {
        // Note: This connection string is temporary and needs to match the db server on the docker container
        string connStr = "server=localhost;user=root;database=db_name;port=3306;password=password";
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
                Console.WriteLine(reader[0]+"\n");
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
}