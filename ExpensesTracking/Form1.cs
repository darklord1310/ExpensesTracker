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
using System.Web;

namespace ExpensesTracking
{
    public partial class Form1 : Form
    {
        // Global variables
        double transportation, meal, others;
        string date, transportDetails, mealDetails, othersDetails;
        string appPath;
        string folderPath;
        SQLiteConnection connection;
        SQLDatabase db;
        List<PieInfo> thePieInfo = new List<PieInfo>();         // contains the info of each of the segment in the pie (color, distribution)
        List<string> tooltip = new List<string>();              // contains the tool tip msg for each of the selection in combo box
        List<comboBoxItems> transportationMode = new List<comboBoxItems>();
        const int positionX = 150;
        const int positionY = 250;
        const int r = 100;

        enum Month { JAN = 1, 
                     FEB,
                     MAR,
                     APR,
                     MAY,
                     JUN,
                     JUL,
                     AUG,
                     SEP,
                     OCT,
                     NOV,
                     DEC};

        public class comboBoxItems
        {
            public string name;
            public double fare;

            public comboBoxItems(string n, double fare)
            {
                name = n;
                this.fare = fare;
            }
        }

        public class MonthlyExpenses
        {
            public double mealExpense;
            public double othersExpense;
            public double transportationExpense;
            public List<dailyExpenses> daily = new List<dailyExpenses>();
        }

        public class dailyExpenses
        {
            public string date;
            public double mealExpense;
            public double othersExpense;
            public double transportationExpense;
        }

        struct PieInfo
        {
            public string desc;
            public float percentage;
            public Color color;
            public Label textbox;

            public PieInfo(string d, float p, Color c, Label txtbox)
            {
                desc = d;
                percentage = p;
                color = c;
                textbox = txtbox;
            }
        };

        public Form1()
        {
            InitializeComponent();
            createDataPath();
            db = new SQLDatabase();
            comboBox1.DropDownClosed += comboBox1_DropDownClosed;
            fillComboBox();
            computeMonthlyChart(DateTime.Now.Month, DateTime.Now.Year);
            clearAllTxtBox();
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            toolTip1.Hide(comboBox1);
        }

        public void fillComboBox()
        {
            try
            {
                    db.createDatabase("TransportationData", appPath);               // create database if not exists
                    connection = db.createDBConnection(Path.Combine(folderPath, "TransportationData.db"), connection);
                    db.connectToDB(connection);
                    db.createTransportationDataTable(connection);
                    string s = "select * from TransportationData";
                    SQLiteCommand command = new SQLiteCommand(s, connection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    transportationMode.Clear();
                    comboBox1.Items.Clear();
                    comboBoxItems item = new comboBoxItems("None", 0.0);
                    transportationMode.Add(item);

                    while (reader.Read())
                    {
                        string des = reader["Description"].ToString();
                        string type = reader["Type"].ToString();
                        string fare = reader["Fares"].ToString();
                        tooltip.Add(des);                               // add the description to tooltip

                        string temp = type + "    " + fare;
                        item = new comboBoxItems(temp, Convert.ToDouble(fare) );
                        transportationMode.Add(item);
                    }

                    foreach(comboBoxItems i in transportationMode)
                    {
                        comboBox1.Items.Add(i.name);
                    }

                    comboBox1.SelectedIndex = 0;                        // set default selected item to "None"
                    db.closeConnection(connection);
                    command.Dispose();
            }
            catch (Exception expmsg)
            {
                MessageBox.Show(expmsg.Message);
            }
        }

        float convertCategoriesToPercentage(double categoryExpenses, MonthlyExpenses expenses)
        {
            float total = (float)expenses.mealExpense + (float)expenses.othersExpense + (float)expenses.transportationExpense;
            float percentage = (float)categoryExpenses / total * 100;          
            return (percentage);
        }

        public void computeMonthlyChart(int month, int year)
        {
            MonthlyExpenses monthExpense = new MonthlyExpenses();
            string filename = generateDailyFilename(month, year);
            textBox1.Text = "Current Month Expenses Chart";
            monthExpense = calculateMonthlyExpenses(monthExpense, filename);

            if(monthExpense.transportationExpense == 0.0 && monthExpense.mealExpense == 0.0 && monthExpense.othersExpense == 0.0)
            {
                // do nothing
            }
            else
            {
                clearAllPieInfo();
                createPieInfo("Transportation", convertCategoriesToPercentage(monthExpense.transportationExpense, monthExpense), Color.Aqua);
                createPieInfo("Meal", convertCategoriesToPercentage(monthExpense.mealExpense, monthExpense), Color.Red);
                createPieInfo("Others", convertCategoriesToPercentage(monthExpense.othersExpense, monthExpense), Color.Purple);
            }
        }

        private void clearAllPieInfo()
        {
            foreach (PieInfo info in thePieInfo)
            {
                this.Controls.Remove(info.textbox);
            }
            thePieInfo.Clear();
        }

        private MonthlyExpenses calculateMonthlyExpenses(MonthlyExpenses monthly, string fileName)
        {
            fileName = fileName + ".sqlite";

            if (File.Exists(Path.Combine(folderPath, fileName)))
            {            
                connection = db.createDBConnection(Path.Combine(folderPath, fileName), connection);
                db.connectToDB(connection);
                string sql = "SELECT * from dailyExpenses";
                SQLiteCommand sqlCommand = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    dailyExpenses temp = new dailyExpenses();
                    temp.date = reader["date"].ToString();
                    temp.transportationExpense = Convert.ToDouble(reader["transportation"]);
                    temp.mealExpense = Convert.ToDouble(reader["meal"]);
                    temp.othersExpense = Convert.ToDouble(reader["others"]);
                    monthly.daily.Add(temp);
                    monthly.mealExpense += temp.mealExpense;
                    monthly.othersExpense += temp.othersExpense;
                    monthly.transportationExpense += temp.transportationExpense;
                }
            }
            else
            {
                monthly.mealExpense = 0.0;
                monthly.othersExpense = 0.0;
                monthly.transportationExpense = 0.0;
            }

            return monthly;
        }

        public void createDataPath()
        {
            appPath = Path.GetDirectoryName(Application.ExecutablePath);  // get the root path of the dir
            folderPath = Path.Combine(appPath, "AppData");                // get the path to the AppData folder

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
        }

        private void clearAllTxtBox()
        {
            mealTxt.Text = "0";
            othersTxt.Text = "0";
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            richTextBox3.Text = "";
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            try 
            {
                date = dateTimePicker1.Value.ToString("dd/MM/yyyy");                             // get the time stamp
                transportation = transportationMode[comboBox1.SelectedIndex].fare;

                if (mealTxt.Text == "")
                    throw new CException("Empty", mealTxt.Name);
                else if(!mealTxt.Text.Any(x => !char.IsLetter(x)))
                    throw new CException("NotNumber", mealTxt.Name);
                else if (othersTxt.Text == "")
                    throw new CException("Empty", othersTxt.Name);
                else if (!othersTxt.Text.Any(x => !char.IsLetter(x)))
                    throw new CException("NotNumber", othersTxt.Name);

                if (Convert.ToDouble(mealTxt.Text) < 0) { throw new CException("NegativeNumber", mealTxt.Name); } else { meal = Convert.ToDouble(mealTxt.Text); }
                if (Convert.ToDouble(othersTxt.Text) < 0) { throw new CException("NegativeNumber", othersTxt.Name); } else { others = Convert.ToDouble(othersTxt.Text); }
                transportDetails = richTextBox1.Text;
                mealDetails = richTextBox2.Text;
                othersDetails = richTextBox3.Text;
                string filename = generateDailyFilename(dateTimePicker1.Value.Month, dateTimePicker1.Value.Year);
                createDataPath();
                addDataToDailyExpenses(filename, appPath, folderPath);
                AddButton.Enabled = true;
                this.Cursor = Cursors.Default;
                computeMonthlyChart(DateTime.Now.Month, DateTime.Now.Year);
                clearAllTxtBox();
            }
            catch (Exception ex)
            {
                AddButton.Enabled = true;
            }
        }

        public class CException : Exception
        {
            public CException(string s, string txtName)
            {
                if (s == "NotNumber")
                {
                    if (txtName == "mealTxt")
                        MessageBox.Show("Meal fees must be a number! If no then put zero.");
                    else if (txtName == "othersTxt")
                        MessageBox.Show("Others fees must be a number! If no then put zero.");
                }
                else if(s == "NegativeNumber")
                {
                    if (txtName == "mealTxt")
                        MessageBox.Show("Meal fees cannot be negative number.");
                    else if (txtName == "othersTxt")
                        MessageBox.Show("Others fees cannot be negative number.");
                }
                else if(s == "Empty")
                {
                    if (txtName == "mealTxt")
                        MessageBox.Show("Meal fees cannot be empty.");
                    else if (txtName == "othersTxt")
                        MessageBox.Show("Others fees cannot be empty.");
                }
            }
        }


        private void addDataToDailyExpenses(string filename, string appPath, string folderPath)
        {
            string daily, trDetails = "", mDetails = "", oDetails = "";
            double tempTransport = 0, tempMeal = 0, tempOthers = 0;
            db.createDatabase(filename, appPath);               // create database if not exists
            filename = filename + ".sqlite";
            connection = db.createDBConnection(Path.Combine(folderPath, filename), connection);
            db.connectToDB(connection);
            db.createDailyExpensesTable(connection);
            this.Cursor = Cursors.WaitCursor;
            AddButton.Enabled = false;
           
            if(db.isRecordExists(connection, date) )
            {
                string sql = "SELECT * from dailyExpenses where date like '" + date + "'";
                SQLiteCommand sqlCommand = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    tempTransport = Convert.ToDouble(reader["transportation"]) + transportation;
                    tempMeal = Convert.ToDouble(reader["meal"]) + meal;
                    tempOthers = Convert.ToDouble(reader["others"]) + others;
                    if (transportDetails != "" && tempTransport != 0.0)
                        trDetails = reader["transportDetails"].ToString() + "\n" + transportDetails;
                    else
                        trDetails = "";

                    if (mealDetails != "" && tempMeal != 0.0)
                        mDetails = reader["mealDetails"].ToString() + "\n" + mealDetails;
                    else
                        mDetails = "";

                    if (othersDetails != "" && tempOthers != 0.0)
                        oDetails = reader["othersDetails"].ToString() + "\n" + othersDetails;
                    else
                        oDetails = "";
                }
                daily = "UPDATE dailyExpenses SET date = @date, transportation = @transportation , meal = @meal, others = @others, transportDetails = @transportDetails, mealDetails = @mealDetails, othersDetails = @othersDetails Where date = @itemToSearch";
                SQLiteCommand command = new SQLiteCommand(daily, connection);
                db.updateDailyExpenses(command, date, date, tempTransport, tempMeal, tempOthers, trDetails, mDetails, oDetails);
            }
            else
            {
                daily = "insert into dailyExpenses (date, transportation, meal, others, transportDetails, mealDetails, othersDetails) values ( @date, @transportation, @meal, @others, @transportDetails, @mealDetails, @othersDetails)";
                SQLiteCommand command = new SQLiteCommand(daily, connection);
                db.addDataToDailyDB(date, transportation, meal, others, transportDetails, mealDetails, othersDetails, command);
            }
            
            db.closeConnection(connection);
        }


        // Generate the filename for daily expenses
        public string generateDailyFilename(int month, int year)
        {
            return (((Month)month).ToString() + " " + ((Month)year).ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DatabaseViewer view = new DatabaseViewer();
        }

        private void createTransportationDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createDataPath();
            TransportationData data = new TransportationData(appPath, folderPath);
            data.ShowDialog();
            
            // refresh tooltip
            tooltip.Clear();
            fillComboBox();
        }

        private void createPieInfo(string desc, float percent, Color c)
        {
            Label text = new System.Windows.Forms.Label();          // create a label respective to pieInfo
            thePieInfo.Add(new PieInfo(desc, percent, c, text));
        }


        private void drawPieChart(int positionX, int positionY, int r)
        {
            float percent1 = 0;
            float percent2 = 0;
            float radius = r;
            int xCenter = positionX, yCenter = positionY;
            Pen myPen = new Pen(Color.Red);
            Graphics myGraphics = this.CreateGraphics();
            float x = xCenter - radius;
            float y = yCenter - radius;
            float width = radius * 2;
            float height = radius * 2;

            foreach (PieInfo info in thePieInfo)
            {
                int index = thePieInfo.IndexOf(info);
                if (index >= 1)
                    percent1 += thePieInfo[index - 1].percentage;
                percent2 += thePieInfo[index].percentage;
                float angle1 = percent1 / 100 * 360;
                float angle2 = percent2 / 100 * 360;

                Brush b = new SolidBrush(thePieInfo[index].color);
                myGraphics.FillPie(b, x, y, width, height, angle1, angle2 - angle1);
            }

            myPen.Color = Color.Black;
            myGraphics.DrawEllipse(myPen, x, y, width, height);

            float xpos = x + width + 20;
            float ypos = y - 40;

            foreach (PieInfo info in thePieInfo)                            // draw the pie info by looping the pieinfo one by one
            {
                Brush b = new SolidBrush(info.color);
                myGraphics.FillRectangle(b, xpos, ypos+50, 30, 30);
                myGraphics.DrawRectangle(myPen, xpos, ypos+50, 30, 30);
                Brush b2 = new SolidBrush(Color.Black);
                setDescriptionTextProperties((int)(xpos + 35), (int)(ypos + 60), info.textbox);
                info.textbox.Text = info.desc + ": " + ((int)info.percentage).ToString() + "%";
                ypos += 35;
            }

            myGraphics.Dispose();
        }


        // set and show the description label location and size according to positionX and positionY
        private void setDescriptionTextProperties(int x, int y, Label lbl)
        {
            lbl.Location = new System.Drawing.Point(x, y);
            lbl.Size = new System.Drawing.Size(110, 13);
            Controls.Add(lbl);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        { 
            drawPieChart(positionX, positionY, r);
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; } // added this line thanks to Andrew's comment
            string text = comboBox1.GetItemText(comboBox1.Items[e.Index]);
            e.DrawBackground();
            using (SolidBrush br = new SolidBrush(e.ForeColor))
            { e.Graphics.DrawString(text, e.Font, br, e.Bounds); }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                if(e.Index > 0)
                    toolTip1.Show(tooltip.ElementAt(e.Index - 1), comboBox1, e.Bounds.Right, e.Bounds.Bottom);   
            }
            e.DrawFocusRectangle();
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
