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
    public partial class TransportationData : Form
    {
        SQLiteConnection transportData;
        SQLDatabase db = new SQLDatabase();
         
        public TransportationData(string appPath, string folderPath)
        {
            InitializeComponent();
            db.createDatabase("TransportationData", appPath);
            transportData = db.createDBConnection(Path.Combine(folderPath, "TransportationData.db"), transportData);
            db.connectToDB(transportData);
            db.createTransportationDataTable(transportData);
            displayDataFromDB(transportData);

        }

        private void displayDataFromDB(SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand("Select * from TransportationData", connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                dataGridView1.Rows.Add(new object[] { 
                    reader.GetValue(reader.GetOrdinal("Type")),  
                    reader.GetValue(reader.GetOrdinal("Description")),
                    reader.GetValue(reader.GetOrdinal("Fares"))
                   });
            }
       }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            AddTransportData add = new AddTransportData();
            add.ShowDialog();

            string t = "insert into TransportationData (Type, Description, Fares) values ( @Type, @Description, @Fares)";
            SQLiteCommand command = new SQLiteCommand(t, transportData);
            this.Cursor = Cursors.WaitCursor;
            if(add.type != "" && add.description != "" && add.fare != 0.0)
            {
                db.addDataToTransportation(add.type, add.description, add.fare, command);
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
                displayDataFromDB(transportData);
            }
            button1.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void TransportationData_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.closeConnection(transportData);
        }

        // edit
        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            string type = "";
            string desc = "";
            double fare = 0.0;

            if(dataGridView1.RowCount > 0)
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                string item = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
                string sql = "SELECT * from TransportationData where Type like '" + item + "'";
                SQLiteCommand sqlCommand = new SQLiteCommand(sql, transportData);
                SQLiteDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    type = reader["Type"].ToString();
                    desc = reader["Description"].ToString();
                    fare = Convert.ToDouble(reader["Fares"]);
                }

                AddTransportData add = new AddTransportData(item, desc, fare);
                add.ShowDialog();

                if (add.type != "" && add.description != "" && add.fare != 0.0)
                {
                    string test_update = "UPDATE TransportationData SET Type = @Type, Description = @Description , Fares = @Fares Where Type = @itemToSearch";
                    SQLiteCommand command = new SQLiteCommand(test_update, transportData);
                    db.updateSpecificDataInDB(command, add.oldType, add.type, add.description, add.fare);
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();
                    displayDataFromDB(transportData);
                }
            }

            button3.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            button2.Enabled = false;
            int rowIndex;

            if(dataGridView1.RowCount != 0)
            {
                rowIndex = dataGridView1.CurrentCell.RowIndex;
                if (dataGridView1.Rows[rowIndex].Cells[0].Value != null)       // check if the selected row is not empty
                {
                    string type = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
                    string del = "DELETE FROM TransportationData WHERE Type = @itemToSearch";
                    SQLiteCommand delete = new SQLiteCommand(del, transportData);
                    db.deleteSpecificFromDB(delete, type);
                }
                SQLiteCommand command = new SQLiteCommand("vacuum;", transportData);
                command.ExecuteNonQuery();
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
                displayDataFromDB(transportData);
            }
            this.Cursor = Cursors.Default;
            button2.Enabled = true;
        }

    }
}
