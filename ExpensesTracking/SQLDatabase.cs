using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace ExpensesTracking
{
    public class SQLDatabase
    {
        // Create the sqlite database according to the DBName
        public void createDatabase(string DBName, string path)
        {            
            try
            {
                if (!DBName.Equals("TransportationData"))
                    DBName = DBName + ".sqlite";                    // concatenate the filename
                else
                    DBName = DBName + ".db";

                string temp = Path.Combine("AppData", DBName);
                if (!File.Exists(Path.Combine(path, temp)))         // if file is not exists
                {
                    SQLiteConnection.CreateFile(DBName);
                    string s = Path.Combine(path, DBName);
                    File.Move(s, Path.Combine(path, temp));
                }

            }
            catch(Exception ex)
            {
               
            }
        }

        // Create the connection for specific database
        public SQLiteConnection createDBConnection(string path, SQLiteConnection m_dbConnection)
        {
            m_dbConnection = new SQLiteConnection("Data Source=" + path + ";Version=3;" + "PRAGMA AUTO_VACUUM = true");
            return m_dbConnection;
        }

        public void addDataToDailyDB(string date, double transportation, double meal, double others, string transportDetails, string mealDetails, string othersDetails, SQLiteCommand command)
        {
            command.Parameters.AddWithValue("@date", date);
            command.Parameters.AddWithValue("@transportation", transportation);
            command.Parameters.AddWithValue("@meal", meal);
            command.Parameters.AddWithValue("@others", others);
            command.Parameters.AddWithValue("@transportDetails", transportDetails);
            command.Parameters.AddWithValue("@mealDetails", mealDetails);
            command.Parameters.AddWithValue("@othersDetails", othersDetails);

            command.ExecuteNonQuery();
        }

        public void addDataToTransportation(string Type, string Description, double Fares, SQLiteCommand command)
        {
            command.Parameters.AddWithValue("@Type", Type);
            command.Parameters.AddWithValue("@Description", Description);
            command.Parameters.AddWithValue("@Fares", Fares);

            command.ExecuteNonQuery();
        }

        // Open the connection for specific database
        public void connectToDB(SQLiteConnection connection)
        {
            connection.Open();
        }

        // Create the database (One time only)
        public void createTransportationDataTable(SQLiteConnection connection)
        {
            string test = "create table if not exists TransportationData (Type text, Description text, Fares double)";
            SQLiteCommand command = new SQLiteCommand(test, connection);
            command.ExecuteNonQuery();
        }

        // Create the database (One time only)
        public void createDailyExpensesTable(SQLiteConnection connection)
        {
            // create daily expenses table with 4 columns (date, transportation, meal, others)
            string daily = "create table if not exists dailyExpenses (date text, transportation double, meal double, others double, transportDetails text, mealDetails text, othersDetails text)";
            SQLiteCommand command = new SQLiteCommand(daily, connection);
            command.ExecuteNonQuery();
        }

        public bool isRecordExists(SQLiteConnection connection, string itemToSearch)
        {
            string temp = "'" + itemToSearch + "'";
            string sql = "SELECT count(*) from dailyExpenses where date like " + temp;
            SQLiteCommand sqlCommand = new SQLiteCommand(sql, connection);

            int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
            if (count == 0)
                return false;
            else
                return true;
        }


        public void updateSpecificDataInDB(SQLiteCommand command, string itemToSearch, string Type, string Description, double Fares)
        {
            command.Parameters.AddWithValue("@itemToSearch", itemToSearch);
            command.Parameters.AddWithValue("@Type", Type);
            command.Parameters.AddWithValue("@Description", Description);
            command.Parameters.AddWithValue("@Fares", Fares);
  
            command.ExecuteNonQuery();
        }

        public void updateDailyExpenses(SQLiteCommand command, string itemToSearch, string date, double transportation, double meal, double others, string transportDetails, string mealDetails, string othersDetails)
        {
            command.Parameters.AddWithValue("@itemToSearch", itemToSearch);
            command.Parameters.AddWithValue("@date", date);
            command.Parameters.AddWithValue("@transportation", transportation);
            command.Parameters.AddWithValue("@meal", meal);
            command.Parameters.AddWithValue("@others", others);

            if(transportDetails != "")
                command.Parameters.AddWithValue("@transportDetails", transportDetails);

            if (mealDetails != "")
                command.Parameters.AddWithValue("@mealDetails", mealDetails);

            if (othersDetails != "")
                command.Parameters.AddWithValue("@othersDetails", othersDetails);

            command.ExecuteNonQuery();
        }

        public void closeConnection(SQLiteConnection m_dbConnection)
        {
            m_dbConnection.Close();
        }

        public void deleteSpecificFromDB(SQLiteCommand command, string itemToSearch)
        {
            command.Parameters.AddWithValue("@itemToSearch", itemToSearch);
            command.ExecuteNonQuery();
        }
    }
}
