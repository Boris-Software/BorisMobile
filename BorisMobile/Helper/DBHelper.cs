using BorisMobile.Models;
using Microsoft.Data.Sqlite;
using SQLite;

namespace BorisMobile.Helper
{
    public class DBHelper
    {
        static SqliteConnection _connection;

        public static SqliteConnection DatabaseConnection
        {
            get
            {
                _connection = new SqliteConnection($"Data Source={Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "BorisSqlite.db3")}");
                _connection.Open();
                return _connection;
            }
        }

        static SQLiteAsyncConnection database;

        public static SQLiteAsyncConnection Database
        {
            get
            {
                if (database == null)
                {
                    CreateDBTables();
                }

                return database;
            }
        }
        public static SQLiteAsyncConnection CreateDBTables()
        {
            database = new SQLiteAsyncConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "BorisSqlite.db3"));

            using (SqliteConnection cn = DatabaseConnection)
            {
                cn.Open();
                using (SqliteCommand cmd = cn.CreateCommand())
                {
                    RunSQL(cmd, @"CREATE TABLE IF NOT EXISTS Attachments(Id INTEGER PRIMARY KEY NOT NULL,
                                                                                IdGuid GUID NOT NULL,
                                                                                UniqueName TEXT NOT NULL,
                                                                                AttachmentData TEXT NOT NULL,
                                                                                Status INT NOT NULL,
                                                                                Repeat INT,
                                                                                FileName TEXT,
                                                                                SubFormIdGuid GUID,
                                                                                IsCopiedFromWorkOrder BIT,
                                                                                PageRepeatListItemId INT,
                                                                                PKGuid GUID
                                                                                )");

                    RunSQL(cmd, @"CREATE TABLE IF NOT EXISTS tblAuditTrailDetails(Id INTEGER PRIMARY KEY NOT NULL,
                                                                                IdGuid GUID NOT NULL,
                                                                                XmlResults TEXT,
                                                                                Modified DATETIME NOT NULL,
                                                                                AuditTrailReason INT NOT NULL
                                                                                )");
                }
            }

            database.CreateTableAsync<Audits>();
            database.CreateTableAsync<AuditsForCustomer>();
            database.CreateTableAsync<AuditsForLocation>();
            database.CreateTableAsync<AuditsInProgress>();
            database.CreateTableAsync<AuditTrail>();
            database.CreateTableAsync<Models.Contacts>();
            database.CreateTableAsync<Customers>();
            database.CreateTableAsync<CustomersForGroup>();
            database.CreateTableAsync<DataOrganisations>();
            database.CreateTableAsync<DataTransferParameters>();
            database.CreateTableAsync<ExtensionChildren>();
            database.CreateTableAsync<Models.Extensions>();
            database.CreateTableAsync<GenericAttachments>();
            database.CreateTableAsync<GenericListDefinitions>();
            database.CreateTableAsync<GenericLists>();
            database.CreateTableAsync<GroupsForUser>();
            database.CreateTableAsync<LocalContacts>();
            database.CreateTableAsync<LocalCustomers>();
            database.CreateTableAsync<LocalListEntries>();
            database.CreateTableAsync<LocalLocations>();
            database.CreateTableAsync<Locations>();
            database.CreateTableAsync<LocationsForUser>();
            database.CreateTableAsync<ReleasedAudits>();
            database.CreateTableAsync<Settings>();
            database.CreateTableAsync<SimpleMessages>();
            database.CreateTableAsync<SupportingWork>();
            database.CreateTableAsync<Translations>();
            database.CreateTableAsync<Users>();
            database.CreateTableAsync<WorkOrderAttachments>();
            database.CreateTableAsync<WorkOrderDefinitions>();
            database.CreateTableAsync<WorkOrders>();
            database.CreateTableAsync<Attachments>();

            return database;
        }

        private static void RunSQL(SqliteCommand cmd, string strSQL)
        {
            try
            {
                // Use INT (not INTEGER) to stop the PK from being an alias for the ROWID (http://www.sqlite.org/lang_createtable.html)
                cmd.CommandText = strSQL;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("App RunSQL: " + ex.Message);
            }
        }
    }


}
