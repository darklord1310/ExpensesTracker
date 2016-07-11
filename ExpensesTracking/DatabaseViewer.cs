using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace ExpensesTracking
{
    public partial class DatabaseViewer : Form
    {
        SQLDatabase db = new SQLDatabase();
        SQLiteConnection connection;
        private string filenameOnly;
        private string dir;
        double expenses = 0.0;

        public DatabaseViewer()
        {
            InitializeComponent();
            openFileBrowser();
            this.Text = filenameOnly.Before(".") + " Expenses Editor";
            handleDatabaseDisplay();
        }

        private void handleDatabaseDisplay()
        {
            try
            {
                if (!dir.Equals("") )
                {
                    connection = db.createDBConnection(dir, connection);
                    db.connectToDB(connection);
                    displayRecordFromDB(connection);
                    this.ShowDialog();
                }
                else
                {
                    totalexpenses.Text = "0";
                    this.Close();
                }
                   
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }    
        }


        private void openFileBrowser()
        {
            //Create a new instance of the OpenFileDialog
            OpenFileDialog dialog = new OpenFileDialog();

            //Set the file filter
            dialog.Filter = "sqlite files (*.sqlite)|*.sqlite";

            //Set Initial Directory
            string p = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) , "AppData");
            dialog.InitialDirectory = p;
            dialog.Title = "Select a record";

            //Present to the user. 
            if (dialog.ShowDialog() == DialogResult.OK)
                dir = dialog.FileName;
            else
                dir = "";

            filenameOnly = dialog.SafeFileName;
        }

        private void displayRecordFromDB(SQLiteConnection connection)
        {
            string date, transport, meal, others, trDetails, oDetails, mDetails, combineTransport, combineMeal, combineOthers;
            double dailyExpenses;
            SQLiteCommand command = new SQLiteCommand("Select * from dailyExpenses", connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                  date = reader.GetValue(reader.GetOrdinal("date")).ToString();
                  transport = reader.GetValue(reader.GetOrdinal("transportation")).ToString();
                  meal = reader.GetValue(reader.GetOrdinal("meal")).ToString();
                  others = reader.GetValue(reader.GetOrdinal("others")).ToString();
                  trDetails = reader.GetValue(reader.GetOrdinal("transportDetails")).ToString();
                  mDetails = reader.GetValue(reader.GetOrdinal("mealDetails")).ToString();
                  oDetails = reader.GetValue(reader.GetOrdinal("othersDetails")).ToString();
                  combineTransport = transport  + "\n\n" + trDetails;
                  combineMeal = meal + "\n\n" + mDetails;
                  combineOthers = others + "\n\n" + oDetails;
                  dailyExpenses = Convert.ToDouble(meal) + Convert.ToDouble(others) + Convert.ToDouble(transport);
                  string[] row = new string[] { date, combineTransport, combineMeal, combineOthers, dailyExpenses.ToString() };
                  expenses += dailyExpenses;
                  dataGridView1.Rows.Add(row);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // Delete button
        private void button1_Click(object sender, EventArgs e)
        {
            /*
            this.Cursor = Cursors.WaitCursor;
            button1.Enabled = false;
            if (dataGridView1.Rows.Count > 0)       // if there is row in datagrid then only perform selected checking
            {
                Int32 selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedRowCount > 0)           // check if there is selected row
                {
                    for (int i = 0; i < selectedRowCount; i++)
                    {
                        if (dataGridView1.SelectedRows[i].Cells[0].Value != null)       // check if the selected row is not empty
                        {
                            string date = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                            string del = "DELETE FROM dailyExpenses WHERE date = @itemToSearch";
                            SQLiteCommand delete = new SQLiteCommand(del, connection);
                            db.deleteSpecificFromDB(delete, date);
                        }
                    }
                }
            }
            SQLiteCommand command = new SQLiteCommand("vacuum;", connection);
            command.ExecuteNonQuery();
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            displayRecordFromDB(connection);
            this.Cursor = Cursors.Default;
            button1.Enabled = true;
            */
        }

        // edit
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void DatabaseViewer_Shown(object sender, EventArgs e)
        {
            totalexpenses.Text = expenses.ToString();
        }
    }
}
