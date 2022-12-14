using System.Data.SQLite;

namespace Libs.Sqlite;

public class Sqlite
{
    private SQLiteConnection Connection { get; }
    
    public Sqlite()
    {
        Connection = new SQLiteConnection($"Data Source={Path.GetFullPath(".\\Sqlite\\data.sqlite")}");
        Connection.Open();
    }

    public string? GetCityNameByInsee(long insee)
    {
        var result = string.Empty;
        var cmd = $"SELECT \"Commune\" FROM t_insee_postal WHERE \"Code INSEE\"={insee}";
        
        var reader = ExecuteReader(cmd);
        while (reader.Read())
        {
            result = reader["Commune"].ToString();
        }
        reader.Close();

        return result;
    }

    private SQLiteDataReader ExecuteReader(string cmd)
    {
        var command = new SQLiteCommand(cmd, Connection);
        return command.ExecuteReader();
    }
}