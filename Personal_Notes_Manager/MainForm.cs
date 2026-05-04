using Microsoft.Data.Sqlite;
using System.Windows.Forms;


namespace Personal_Notes_Manager
{

    public partial class MainForm : Form
    {
        private SqliteConnection connection;
        private int currIndex = 0;
        public MainForm()
        {


            InitializeComponent();
            InitialiseDatabase();

        }
        private void InitialiseDatabase()
        {
            OpenDatabaseConnection();
            CreateDatabase();
            ReadDatabase();

        }

        private void OpenDatabaseConnection()
        {
            connection = new SqliteConnection("Data Source=notes.db");
            connection.Open();
        }

        private void CreateDatabase()
        {

            string sqlQuery = "CREATE TABLE IF NOT EXISTS notes (id INTEGER PRIMARY KEY,name TEXT NOT NULL,text TEXT);";
            var command = new SqliteCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void ReadDatabase()
        {
            lstNotes.Items.Clear();
            var command = new SqliteCommand("SELECT * FROM notes", connection);
            var data = command.ExecuteReader();
            while (data.Read())
            {
                Note noteItem = new Note();
                noteItem.Name = data["name"].ToString();
                noteItem.Id = Convert.ToInt32(data["id"]);
                noteItem.Text = data["text"].ToString();

                lstNotes.Items.Add(noteItem);
            }
            if (lstNotes.Items.Count > 0)
            {
                if (currIndex >= lstNotes.Items.Count)
                {
                    lstNotes.SelectedIndex = currIndex - 1;
                    currIndex -= 1;
                    return;
                }
                lstNotes.SelectedIndex = currIndex;
                return;
            }
        }

        private void CreateNote(object sender, EventArgs e)
        {
            string sqlQuery = "INSERT INTO notes (name, text) VALUES (@name, @text);";
            var command = new SqliteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@name", "New Note");
            command.Parameters.AddWithValue("@text", "");
            command.ExecuteNonQuery();
            currIndex = lstNotes.Items.Count;
            ReadDatabase();

        }


        private void GetNote(object sender, EventArgs e)
        {
            Note selected = (Note)lstNotes.SelectedItem;
            if (selected == null) { return; }
            currIndex = lstNotes.SelectedIndex;
            rtbNoteBody.Text = "";
            txtNoteTitle.Text = "";
            rtbNoteBody.Text = selected.Text;
            txtNoteTitle.Text = selected.Name;

        }

        private void DeleteNote(object sender, EventArgs e)
        {
            Note selected = (Note)lstNotes.SelectedItem;
            if (selected == null) { return; }
            var confirmDeletion = MessageBox.Show("Are you sure you want to delete?", "Confirmation", MessageBoxButtons.YesNo);
            if (confirmDeletion == DialogResult.No) { return; }
            string sqlQuery = "DELETE FROM notes WHERE id = @noteid;";
            var command = new SqliteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@noteid", selected.Id);
            var data = command.ExecuteNonQuery();
            ReadDatabase();
        }

        private void SaveNote(object sender, EventArgs e)
        {
            Note selected = (Note)lstNotes.SelectedItem;
            if (selected == null) { return; }
            string sqlQuery = "UPDATE notes SET text = @text,name = @name WHERE id = @id;";
            var command = new SqliteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", selected.Id);
            command.Parameters.AddWithValue("@name", txtNoteTitle.Text);
            command.Parameters.AddWithValue("@text", rtbNoteBody.Text);
            command.ExecuteNonQuery();
            ReadDatabase();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
    }
}
