using Microsoft.Data.Sqlite;

public class NoteRepository
{
    private readonly string _connectionString;

    public NoteRepository(string connectionString)
    {
        _connectionString = connectionString;
        InitialiseDatabase();
    }

    private void InitialiseDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = new SqliteCommand(
            "CREATE TABLE IF NOT EXISTS notes (id INTEGER PRIMARY KEY, name TEXT NOT NULL, text TEXT);",
            connection);
        command.ExecuteNonQuery();
    }

    public List<Note> GetAll()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = new SqliteCommand("SELECT * FROM notes", connection);
        var reader = command.ExecuteReader();
        var notes = new List<Note>();
        while (reader.Read())
        {
            notes.Add(new Note
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Text = reader.GetString(2)
            });
        }
        return notes;
    }

    public Note? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = new SqliteCommand("SELECT * FROM notes WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new Note
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Text = reader.GetString(2)
        };
    }

    public Note Create()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = new SqliteCommand(
            "INSERT INTO notes (name, text) VALUES (@name, @text) RETURNING id;",
            connection);
        command.Parameters.AddWithValue("@name", "New Note");
        command.Parameters.AddWithValue("@text", "");
        var id = Convert.ToInt32(command.ExecuteScalar());
        return new Note { Id = id };
    }

    public bool Update(int id, Note updated)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = new SqliteCommand(
            "UPDATE notes SET name = @name, text = @text WHERE id = @id",
            connection);
        command.Parameters.AddWithValue("@name", updated.Name);
        command.Parameters.AddWithValue("@text", updated.Text);
        command.Parameters.AddWithValue("@id", id);
        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = new SqliteCommand("DELETE FROM notes WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        return command.ExecuteNonQuery() > 0;
    }
}
