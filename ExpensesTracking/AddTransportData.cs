using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExpensesTracking
{
    public partial class AddTransportData : Form
    {
        public string type;
        public double fare;
        public string description;
        public string oldType;

        // add new transport data
        public AddTransportData()
        {
            InitializeComponent();
            this.Text = "Add new transport data";
        }

        // edit transport data
        public AddTransportData(string type, string desc, double fare)
        {
            InitializeComponent();
            this.Text = "Edit transport data";
            button1.Text = "OK";
            typeTxt.Text = type;
            oldType = type;
            faresTxt.Text = fare.ToString();
            desTxt.Text = desc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                type = typeTxt.Text;
                fare = Convert.ToDouble(faresTxt.Text);
                description = desTxt.Text;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            type = "";
            fare = 0.0;
            description = "";
            this.Close();
        }

        private void AddTransportData_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
        }

        public class CException : Exception
        {
            public CException(string s, string txtName)
            {
                if (s == "NotNumber")
                {
                    if (txtName == "faresTxt")
                        MessageBox.Show("Fares must be a number!");
                }
                else if (s == "NegativeNumber")
                {
                    if (txtName == "faresTxt")
                        MessageBox.Show("Fares cannot be negative number!");
                }
                else if (s == "Empty")
                {
                    if (txtName == "typeTxt")
                        MessageBox.Show("Type cannot be empty!");
                    else if (txtName == "faresTxt")
                        MessageBox.Show("Fares cannot be empty!");
                }

            }
        }
    }
}
