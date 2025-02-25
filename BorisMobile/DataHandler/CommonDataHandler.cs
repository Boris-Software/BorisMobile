using BorisMobile.DataHandler.Data;
using BorisMobile.Helper;
using BorisMobile.XML;
using Microsoft.Data.Sqlite;
using System.Xml;
using System.Xml.Linq;
using static BorisMobile.DataHandler.Data.DataEnum;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class CommonDataHandler
    {
        public ListEntry GetListEntry(int listEntryId)
        {
            ListEntry listEntry = GetListEntryFromSQL("SELECT Desc, XmlDoc, List FROM GenericLists WHERE Id = " + listEntryId, true);
            if (listEntry != null && listEntry.Attributes != null && !listEntry.Attributes.ContainsKey("id")) // 040721
            {
                listEntry.Attributes.Add("id", listEntryId.ToString()); // substitution stuff was assuming strings in the hashtable.
            }

            return listEntry;
        }

        private ListEntry GetListEntryFromSQL(string sql, bool setListId)
        {
            ListEntry listEntry = null;
            using (SqlCeCommand listEntryCommand = GetCommandObject(sql))
            {
                using (SqlCeDataReader reader = listEntryCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string desc = reader["Desc"] == DBNull.Value ? "" : reader["Desc"].ToString();
                        listEntry = new ListEntry(desc);
                        XmlDocument xmlDoc = null;
                        if (reader["XmlDoc"] != DBNull.Value)
                        {
                            string xmlDocText = (string)reader["XmlDoc"];
                            if (!string.IsNullOrEmpty(xmlDocText))
                            {
                                xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(xmlDocText);
                                listEntry.XmlDocument = xmlDoc; // 040721 Store this so we can get row data (Aquatreat temperatures)
                            }
                        }
                        listEntry.Attributes = XML.XmlUtils.GetAllAttributes(xmlDoc, desc);
                        if (setListId)
                        {
                            listEntry.ListId = GetIntFromReader(reader["List"]); // 260120
                        }
                    }
                }
            }
            return listEntry;
        }

        public SqlCeCommand GetSimpleListCommandObject(int listId)
        {
            SqlCeCommand command = GetCommandObject(
                @"SELECT GenericLists.Id as Id, Desc FROM GenericLists
                WHERE List = " + listId + " ORDER BY Seq, Desc");
            //AddIntParam(command, "@ListId", listId);
            command.Prepare();
            return command;
        }

        public async Task<IdAndDescriptionCollection> GetListEntriesForList(int listId, string activityId, RecordFilterFunction filterFunction)
        {
            SqlCeCommand command = GetSimpleListCommandObject(listId);
            IdAndDescriptionCollection listEntries = new IdAndDescriptionCollection();
            if (command != null)
            {
                SqlCeDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    IdAndDescriptionListItem listData = new IdAndDescriptionListItem(GetIntFromReader(reader["Id"]), reader["Desc"] == DBNull.Value ? "" : reader["Desc"].ToString());
                    Guid localGuid = GetLocalListEntryFromListItemId(listData.Id);
                    if (localGuid != Guid.Empty)
                    {
                        if (GetLocalListEntryStatusId(localGuid) == (int)LocalEntityStatusEnum.DELETED)
                            continue;
                    }
                    if (!string.IsNullOrEmpty(activityId))
                    {
                        string listActivityId = await GetAttributeForListEntry(listData.Id, "activityId");
                        if (!string.IsNullOrEmpty(listActivityId) && (listActivityId == activityId))
                            listEntries.Add(listData);
                    }
                    else if (filterFunction != null)
                    {
                        if (filterFunction(reader))
                        {
                            listEntries.Add(listData);
                        }
                    }
                    else
                        listEntries.Add(listData);
                }
                reader.Close();
                command.Dispose();
            }
            return listEntries;
        }

        public async Task<string> GetAttributeForListEntry(int listEntryId, string attName)
        {
            if (attName == "score")
            {
                int id = await GetScoreForListEntry(listEntryId);
                string stringId = id.ToString();
                return stringId;
            }

            return await GetAttributeForListEntryOther(listEntryId, attName);
        }

        public async Task<string> GetAttributeForListEntryOther(int listEntryId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM GenericLists WHERE Id = " + listEntryId, attName);
        }

        public async Task<int> GetScoreForListEntry(int listEntryId)
        {
            return GetIntFromDataSql("SELECT Score FROM GenericLists WHERE Id = " + listEntryId);
        }

        public int GetLocalListEntryStatusId(Guid guid)
        {
            using (SqlCeCommand command = new SqlCeCommand("SELECT StatusId FROM LocalListEntries WHERE IdGuid = @guid", DBHelper.DatabaseConnection))
            {
                AddGuidParam(command, "guid", guid);
                command.Prepare();
                return GetIntFromDataSqlCommand(command, -1);
            }
        }
        internal Guid GetLocalListEntryFromListItemId(int id)
        {
            SqlCeCommand command = GetCommandObject("SELECT IdGuid FROM LocalListEntries WHERE ListEntryId = " + id);
            Guid localIdGuid = GetGuidFromDataSqlCommand(command);
            return localIdGuid;
        }

        public async Task<string> GetAttributeFromDataSql(string sqlString, string attName)
        {
            string xmlText = await GetStringFromDataSql(sqlString);
            if (!string.IsNullOrEmpty(xmlText))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlText);
                if (XmlUtils.VerifyName(attName)) // otherwise we get a crash below in SelectSingleNode
                {
                    XmlNode node = xmlDoc.SelectSingleNode("//@" + attName);
                    if (node != null)
                    {
                        return node.Value;
                    }
                }
            }
            return null;
        }

        public async Task<string> GetStringFromDataSql(string strSql)
        {
            object sqlResult = GetScalarResult(strSql);
            if (sqlResult == System.DBNull.Value || (sqlResult == null))
            {
                return "";
            }
            return sqlResult.ToString();
        }

        public object GetScalarResult(string strSql)
        {
            //Android.Util.Log.Info("DB", "GetScalarResult SQL: " + strSql);
            using (SqlCeCommand command = new SqlCeCommand(strSql, DBHelper.DatabaseConnection))
            {
                object result = command.ExecuteScalar();
                //if (result != null)
                //{
                //    Android.Util.Log.Info("DB", "GetScalarResult returns: " + result.ToString());
                //}
                //else
                //{
                //    Android.Util.Log.Info("DB", "GetScalarResult returns NULL");
                //}
                return result;
            }


        }
        public SqlCeCommand GetCommandObject(string strSql)
        {
            return new SqlCeCommand(strSql, DBHelper.DatabaseConnection);
        }

        public int GetIntFromReader(object result)
        {
            if (result is Int16 || result is Int32 || result is Int64)
                return int.Parse(result.ToString());
            else if (result is string)
            {
                int res = -1;
                int.TryParse(result.ToString(), out res);
                return res;
            }
            else
            {
                return -1;
            }
        }

        public int GetIntFromDataSql(string strSql)
        {
            return GetIntFromDataSql(strSql, 0); // because of an old bug in here (sqlResult null test), 0 is the default rather than -1
        }

        public int GetIntFromDataSql(string strSql, int defaultValue)
        {
            object sqlResult = GetScalarResult(strSql);
            if (sqlResult == System.DBNull.Value || (sqlResult == null))
            {
                return defaultValue;
            }
            return Convert.ToInt32(sqlResult);
        }

        public Guid GetGuidFromDataSqlCommand(SqlCeCommand command)
        {
            using (command)
            {
                object sqlResult = command.ExecuteScalar();
                if (sqlResult == System.DBNull.Value || (sqlResult == null))
                {
                    return Guid.Empty;
                }
                if (sqlResult is Guid)
                {
                    return (Guid)sqlResult;
                }
                return new Guid(sqlResult.ToString());
            }
        }

        public async Task<string> GetAttributeForUser(int userId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM Users WHERE Id = " + userId, attName);
        }
        public async Task<string> GetAttributeForCustomerID(int customerId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM Customers WHERE Id = " + customerId, attName);
        }

        public async Task<string> GetAttributeForLocationID(int locationId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM Locations WHERE Id = " + locationId, attName);
        }

        //public string GetAttributeFromDataSql(string sqlString, string attName)
        //{
        //    string xmlText = GetStringFromDataSql(sqlString);
        //    if (!string.IsNullOrEmpty(xmlText))
        //    {
        //        XmlDocument xmlDoc = new XmlDocument();
        //        xmlDoc.LoadXml(xmlText);
        //        if (XmlUtils.VerifyName(attName)) // otherwise we get a crash below in SelectSingleNode
        //        {
        //            XmlNode node = xmlDoc.SelectSingleNode("//@" + attName);
        //            if (node != null)
        //            {
        //                return node.Value;
        //            }
        //        }
        //    }
        //    return null;
        //}

        public void AddGuidParam(SqlCeCommand command, string name, Guid value)
        {
            command.Parameters.Add(name, Microsoft.Data.Sqlite.SqliteType.Text);
            command.Parameters[name].Value = value;
        }
        public void AddTextParam(SqlCeCommand command, string name, string value)
        {
            command.Parameters.Add(name, Microsoft.Data.Sqlite.SqliteType.Text);
            command.Parameters[name].Value = value;
        }

        public static void AddIntParam(SqlCeCommand command, string paramName, int value)
        {
            command.Parameters.Add(paramName, Microsoft.Data.Sqlite.SqliteType.Integer);
            command.Parameters[paramName].Value = value;
        }

        public static void AddIntParamNull(SqliteCommand command, string paramName)
        {
            command.Parameters.AddWithValue(paramName, DBNull.Value); // Handle NULL values
        }

        public static void AddDateTimeParam(SqlCeCommand command, string paramName, DateTime value)
        {
            command.Parameters.AddWithValue(paramName, value.ToString("yyyy-MM-dd HH:mm:ss")); // Store DateTime as text in SQLite
            //command.Parameters.Add(paramName, Microsoft.Data.Sqlite.SqliteType.);
            //command.Parameters[paramName].Value = value;
        }

        public object GetScalarResultFromCommand(SqlCeCommand command)
        {
            return command.ExecuteScalar();
        }
        public int GetIntFromDataSqlCommand(SqlCeCommand command, int defaultValue)
        {
            object sqlResult = GetScalarResultFromCommand(command);
            if (sqlResult == null)
            {
                return defaultValue;
            }
            int result = -1;
            if (int.TryParse(sqlResult.ToString(), out result))
            {
                return result;
            }
            return defaultValue;
        }

        public static Guid GetGuidFromReader(object result)
        {
            if (result is Guid)
                return (Guid)result;
            else if (result is string)
            {
                Guid res = Guid.Empty;
                Guid.TryParse(result.ToString(), out res);
                return res;
            }
            else
            {
                return Guid.Empty;
            }
        }


        

        

    }
}