using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseCreatorLib {
    public class DataBaseCreator {
        public static string GetPrefix(int number) {
            var res = number.ToString();
            res = res.PadLeft(3, '0');
            return res;
        }
        public static void CreateBase(string dataBaseName) {

            var connection = new SqlConnection("data source=(localdb)\\mssqllocaldb;integrated security=SSPI");
            connection.Open();
            var isExistsCommand = new SqlCommand();
            isExistsCommand.Connection = connection;
            isExistsCommand.CommandText = @"DECLARE @dbname nvarchar(128)
SET @dbname = N'" + dataBaseName + @"'
IF (not EXISTS(SELECT name 
FROM [master].sys.databases 
WHERE (name = @dbname)))
 BEGIN
  SET @dbname = QUOTENAME(@dbname)
  EXEC('CREATE DATABASE '+ @dbname)
 END";
            //decimal decimalValue = 111111111987654321;
            //isExistsCommand.Parameters.Add(new SqlParameter("@p1", decimalValue));
            isExistsCommand.ExecuteNonQuery();
        }
        public static void CreateSQLDataBaseIfNotExists(string solutionName, out string dataBaseName, bool isUserProject = false) {
            var dayNumber = 365 - DateTime.Today.DayOfYear;
            var prefix = GetPrefix(dayNumber);
            dataBaseName = string.Format("d{0}-{1}", prefix, solutionName);
            if(isUserProject) {
                dataBaseName = dataBaseName + "usr";
            }
            CreateBase(dataBaseName);

        }
    }
}
