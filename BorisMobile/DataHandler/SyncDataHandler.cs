using BorisMobile.Models;
using BorisMobile.Repository;
using BorisMobile.Utilities;
using BorisMobile.XML;
using System.Collections.ObjectModel;
using System.Xml;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class SyncDataHandler
    {
        CommonDataHandler commonDataHandler;

        public SyncDataHandler()
        {
            commonDataHandler = new CommonDataHandler();

        }
        public List<int> ListIdsForJobs(WOFilter woFilter)
        {
            List<int> listIds = new List<int>();
            using (SqlCeCommand command = commonDataHandler.GetCommandObject(string.Format("SELECT XmlDoc FROM Locations WHERE Id IN (SELECT DISTINCT LocationId FROM WorkOrders {0} UNION SELECT DISTINCT LocationId FROM AuditsInProgress) AND XmlDoc IS NOT NULL UNION SELECT XmlDoc FROM Customers WHERE Id IN (SELECT DISTINCT CustomerId FROM WorkOrders {0} UNION SELECT DISTINCT CustomerId FROM AuditsInProgress) AND XmlDoc IS NOT NULL ", woFilter.FilterText)))
            {
                if (woFilter.StartDateBefore != DateTime.MaxValue)
                {
                    AddDateTimeParam(command, "@StartDateBefore", woFilter.StartDateBefore);
                }
                if (woFilter.StartDateAfter != DateTime.MinValue)
                {
                    AddDateTimeParam(command, "@StartDateAfter", woFilter.StartDateAfter);
                }
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != System.DBNull.Value && reader[0] != null && !string.IsNullOrEmpty((string)reader[0])) // Missing root element 250423
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml((string)reader[0]);
                            foreach (XmlAttribute att in xmlDoc.SelectNodes("//@*[contains(name(), 'ListId')]"))
                            {
                                int listId = -1;
                                if (Int32.TryParse(att.Value, out listId))
                                {
                                    if (listId > 0 && listIds.IndexOf(listId) == -1)
                                    {
                                        listIds.Add(listId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int startIndex = 0;
            do
            {
                startIndex = AddRelatedLists(listIds, startIndex);
            } while (startIndex != -1);
            return listIds;
        }
        public async Task SetAttachmentAsDownloaded(Guid idGuid, bool genericAttachments)
        {
            await SetAttachmentDownloadStatus(idGuid, genericAttachments, 0);
        }

        public async Task SetAttachmentDownloadStatus(Guid idGuid, bool genericAttachments, int needToDownload)
        {
            string tableType = genericAttachments ? "Generic" : "WorkOrder";
            using (SqlCeCommand attachmentsCommand = commonDataHandler.GetCommandObject(
                            string.Format(@"UPDATE {0}Attachments
                                SET NeedToDownload = {1} WHERE IdGuid = '{2}'", tableType, needToDownload,idGuid)))
            {
                //AddGuidParam(attachmentsCommand, "@IdGuid", idGuid);
                attachmentsCommand.Prepare();
                attachmentsCommand.ExecuteNonQuery();
            }
        }
        public Models.Attachments GetNextAttachmentToDownloadForEntity(XmlElement signInPayload, string entityType, int entityId)
        {
            using (SqlCeCommand command = commonDataHandler.GetCommandObject(string.Format("SELECT IdGuid, ShortFileName FROM tblGenericAttachments WHERE NeedToDownload = 1 AND EntityType = '{0}' AND EntityId = {1} ORDER BY IdGuid", entityType, entityId)))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid idGuid = GetGuidFromReader(reader["IdGuid"]);
                        string shortFileName = (string)reader["ShortFileName"];
                        if (entityType == "LI")
                        {
                            shortFileName = Misc.ModifyFileNameBeforeExtension(shortFileName, "_" + idGuid);
                        }
                        if (entityType == "MD") // check app version
                        {
                            XmlElement deviceInfo = (signInPayload == null) ? null : (XmlElement)signInPayload.SelectSingleNode("//Device");
                            string appVer = XmlUtils.StringAtt(deviceInfo, "appVer", "");
                            if (!string.IsNullOrEmpty(appVer) && shortFileName.Contains(appVer))
                            {
                                continue;
                            }
                        }
                        return new Models.Attachments(idGuid, shortFileName);
                    }
                }
            }
            return null;
        }
        
        public void AddGuidParam(SqlCeCommand command, string name, Guid value)
        {

            command.Parameters.AddWithValue(name, value);
        }
        public Models.Attachments GetNextAttachmentToDownload(bool genericAttachments, XmlElement signInPayload, string additionalSql, WOFilter woFilter)
        {
            try
            {
                string tableType = genericAttachments ? "Generic" : "WorkOrder";
                string subClause = genericAttachments ? string.Format(@"AND  (
                                                                    ( EntityType = 'CU' AND (   (EntityId IN (SELECT CustomerId FROM WorkOrders {0})) OR (EntityId IN (SELECT CustomerId FROM AuditsInProgress))))
                                                                    OR ( EntityType = 'LO' AND ((EntityId IN (SELECT LocationId FROM WorkOrders {0})) OR (EntityId IN (SELECT LocationId FROM AuditsInProgress))))
                                                                    OR ( EntityType = 'CF') OR ( EntityType = 'IM') OR ( EntityType = 'MD') OR ( EntityType = 'US')
                                                                    " + additionalSql + ")", woFilter.FilterText) : "";
                using (SqlCeCommand command = commonDataHandler.GetCommandObject(string.Format("SELECT IdGuid, ShortFileName{2} FROM {0}Attachments WHERE NeedToDownload = 1 {1}  ORDER BY IdGuid", tableType, subClause,
                    genericAttachments ? ", EntityType, EntityId" : ""
                    )))
                {
                    if (genericAttachments && !string.IsNullOrEmpty(woFilter.FilterText))
                    {
                        if (woFilter.StartDateBefore != DateTime.MaxValue)
                        {
                            AddDateTimeParam(command, "@StartDateBefore", woFilter.StartDateBefore);
                        }
                        if (woFilter.StartDateAfter != DateTime.MinValue)
                        {
                            AddDateTimeParam(command, "@StartDateAfter", woFilter.StartDateAfter);
                        }
                    }

                    using (SqlCeDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid idGuid = GetGuidFromReader(reader["IdGuid"]);
                            string shortFileName = (string)reader["ShortFileName"];
                            if (genericAttachments)
                            {
                                string entityType = (string)reader["EntityType"];
                                if (entityType == "LI")
                                {
                                    shortFileName = Misc.ModifyFileNameBeforeExtension(shortFileName, "_" + idGuid);
                                }
                                if (entityType == "MD") // check app version
                                {
                                    XmlElement deviceInfo = (signInPayload == null) ? null : (XmlElement)signInPayload.SelectSingleNode("//Device");
                                    string appVer = XmlUtils.StringAtt(deviceInfo, "appVer", "");
                                    if (!string.IsNullOrEmpty(appVer) && shortFileName.Contains(appVer))
                                    {
                                        continue;
                                    }
                                }
                            }
                            return new Models.Attachments(idGuid, shortFileName);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
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

        public void AddDateTimeParam(SqlCeCommand command, string name, DateTime value)
        {
            command.Parameters.AddWithValue(name, value);

        }

        private int AddRelatedLists(List<int> listIds, int startIndex)
        {
            try
            {
                if (listIds == null)
                {
                    return -1;
                }
                List<int> newListIds = new List<int>();
                int existingCount = listIds.Count;
                for (int i = startIndex; i < existingCount; i++)
                {
                    int oneListId = listIds[i]; // Because otherwise the enumerator gets invalidated
                                                // 170321 Should be using newListIds
                                                //AddListIdAttributes(listIds, "SELECT XmlDocument FROM GenericListDefinitions WHERE Id  = " + oneListId);
                                                //AddListIdAttributes(listIds, "SELECT XmlDocument FROM GenericLists WHERE ListId  = " + oneListId);
                    AddListIdAttributes(listIds, newListIds, "SELECT XmlDoc FROM GenericListDefinitions WHERE Id  = " + oneListId);
                    AddListIdAttributes(listIds, newListIds, "SELECT XmlDoc FROM GenericLists WHERE List  = " + oneListId);
                }
                foreach (int oneNew in newListIds)
                {
                    listIds.Add(oneNew);
                }
                if (newListIds.Count > 0)
                {
                    return existingCount;
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;

            }
        }
        private void AddListIdAttributes(List<int> listIds, List<int> newListIds, string sql)
        {
            using (SqlCeCommand command = commonDataHandler.GetCommandObject(sql))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != System.DBNull.Value && reader[0] != null && !string.IsNullOrEmpty((string)reader[0])) // 250423 Missing root element
                        {
                            string xml = (string)reader[0];
                            if (!string.IsNullOrEmpty(xml))
                            {
                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(xml);
                                foreach (XmlAttribute att in xmlDoc.SelectNodes("//@*[contains(name(), 'ListId')]"))
                                {
                                    int listId = -1;
                                    if (Int32.TryParse(att.Value, out listId))
                                    {
                                        if (listId > 0 && listIds.IndexOf(listId) == -1 && newListIds.IndexOf(listId) == -1)
                                        {
                                            newListIds.Add(listId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public async Task<int> GetUserIdForUsername(string username)
        {
            IRepo<Models.Users> dataTransferParameterRepo = new Repo<Models.Users>(App.Database);

            var userobjList = await dataTransferParameterRepo.Get();
            var userobj = userobjList.Where(X => X.UserName == username).FirstOrDefault();

            return userobj.Id;

        }

        public Collection<Attachments> GetGenericAttachmentsForEntity(string entityType, int entityId)
        {
            string sql = string.Format("SELECT IdGuid, ShortFileName FROM GenericAttachments WHERE NeedToDownload = 0 AND EntityType = '{0}'", entityType);
            if (entityId != -1)
            {
                sql += " AND EntityId = " + entityId;
            }
            return GetAttachmentsFromSQL(sql);
        }
        public void DeleteGenericAttachment(string attachmentsDir, Guid idGuid)
        {
            DeleteAttachment(attachmentsDir, idGuid, "GenericAttachments");
        }
        private void DeleteAttachment(string attachmentsDir, Guid idGuid, string tableName)
        {
            SqlCeCommand command = commonDataHandler.GetCommandObject(string.Format("SELECT ShortFileName FROM {0} WHERE IdGuid = @idGuid", tableName, idGuid));
            AddGuidParam(command, "idGuid", idGuid);
            string fileName = GetStringFromDataSqlCommand(command); // !!!!!! MD - does this have { } surrounding it? May well be failing DOC001. Actually, should be OK because it is a guid rather than a string representation with a { }
            if (!string.IsNullOrEmpty(fileName))
            {
                IO.SafeFileDelete(attachmentsDir + Path.DirectorySeparatorChar + fileName);
            }
        }

        public string GetStringFromDataSqlCommand(SqlCeCommand command)
        {
            object sqlResult = GetScalarResultFromCommand(command);
            if (sqlResult == null || sqlResult == System.DBNull.Value)
            {
                return "";
            }
            return sqlResult.ToString();
        }
        public object GetScalarResultFromCommand(SqlCeCommand command)
        {
            return command.ExecuteScalar();
        }
        public async Task<XmlElement> GetUserAndAppOptions(XmlConfigDoc configDoc,int userId)
        {
            XmlElement userOptions = configDoc.XmlDocument.CreateElement("User");
            userOptions.SetAttribute("downloadAllCustomerAttachments", await GetAttributeForUser(userId, "downloadAllCustomerAttachments"));
            userOptions.SetAttribute("downloadAllLocationAttachments", await GetAttributeForUser(userId, "downloadAllLocationAttachments"));
            userOptions.SetAttribute("downloadAllListItemAttachments", await GetAttributeForUser(userId, "downloadAllListItemAttachments"));
            return userOptions;
        }
        public async Task<string> GetAttributeForUser(int userId, string attName)
        {
            return await commonDataHandler.GetAttributeFromDataSql("SELECT XmlDoc FROM Users WHERE Id = " + userId, attName);
        }
        
        public string GetStringFromDataSql(string strSql)
        {
            object sqlResult = commonDataHandler.GetScalarResult(strSql);
            if (sqlResult == System.DBNull.Value || (sqlResult == null))
            {
                return "";
            }
            return sqlResult.ToString();
        }
        
        public Collection<Attachments> GetUnwantedGenericAttachmentsBasic(bool downloadAllCustomerAttachments, bool downloadAllLocationAttachments)
        {
            string sql = @"SELECT IdGuid, ShortFileName FROM GenericAttachments WHERE NeedToDownload = 0 ";
            string sqlCust = "(EntityType = 'CU' AND EntityId NOT IN (SELECT CustomerId FROM WorkOrders) AND EntityId NOT IN (SELECT CustomerId FROM AuditsInProgress))";
            string sqlLoc = "(EntityType = 'LO' AND EntityId NOT IN (SELECT LocationId FROM WorkOrders)  AND EntityId NOT IN (SELECT LocationId FROM AuditsInProgress))";
            if (!downloadAllCustomerAttachments)
            {
                sql += " AND ( " + sqlCust;
                if (!downloadAllLocationAttachments)
                {
                    sql += " OR " + sqlLoc;
                }
                sql += ")";
            }
            else if (!downloadAllLocationAttachments)
            {
                sql += " AND ( " + sqlLoc + ")";
            }
            else
            {
                sql += " AND 1 = 0";
            }
            return GetAttachmentsFromSQL(sql);
            //            return GetAttachmentsFromSQL(@"SELECT IdGuid, ShortFileName FROM tblGenericAttachments WHERE NeedToDownload = 0 AND 
            //                                (
            //                                    (EntityType = 'CU' AND EntityId NOT IN (SELECT CustomerId FROM tblWorkOrders) AND EntityId NOT IN (SELECT CustomerId FROM tblAuditsInProgress))
            //                                    OR
            //                                    (EntityType = 'LO' AND EntityId NOT IN (SELECT LocationId FROM tblWorkOrders)  AND EntityId NOT IN (SELECT LocationId FROM tblAuditsInProgress))
            //                                )");
        }

        private Collection<Attachments> GetAttachmentsFromSQL(string sql)
        {
            //InfoLog("GetAtts: " + sql);
            Collection<Attachments> attachments = new Collection<Attachments>();
            using (SqlCeCommand command = commonDataHandler.GetCommandObject(sql))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Attachments attachment = new Attachments(GetGuidFromReader(reader["IdGuid"]), (string)reader["ShortFileName"]);
                        attachments.Add(attachment);
                    }
                }
            }
            return attachments;
        }

        public void SetAttachmentAsNotDownloaded(Guid idGuid, bool genericAttachments)
        {
            SetAttachmentDownloadStatus(idGuid, genericAttachments, 1);
        }

        public class WOFilter
        {
            private string m_filterText;
            private DateTime m_startDateBefore = DateTime.MaxValue;
            private DateTime m_startDateAfter = DateTime.MinValue;

            public WOFilter(XmlElement configSettings, int userId)
            {
                m_startDateBefore = DateTime.MaxValue;
                m_startDateAfter = DateTime.MinValue;
                m_filterText = "";
                if (configSettings != null)
                {
                    if (XmlUtils.BoolAtt(configSettings, "woExcludeGroupOnly", false)) // exclude jobs that are only relevant to the user because of the group
                    {
                        if (!string.IsNullOrEmpty(m_filterText))
                        {
                            m_filterText += " AND ";
                        }
                        m_filterText += "(GroupId IS NULL OR (UserId IS NOT NULL AND UserId = " + userId + "))";
                    }
                    string woBeforeOffset = XmlUtils.StringAtt(configSettings, "woBeforeOffset", "");
                    string woAfterOffset = XmlUtils.StringAtt(configSettings, "woAfterOffset", "");
                    if (!string.IsNullOrEmpty(woBeforeOffset))
                    {
                        if (!string.IsNullOrEmpty(m_filterText))
                        {
                            m_filterText += " AND ";
                        }
                        m_filterText += "WorkOrderDate < @StartDateBefore ";
                        m_startDateBefore = Chrono.DateTimeAddOffset(Chrono.DayEndForDate(DateTime.Now), woBeforeOffset);
                    }
                    if (!string.IsNullOrEmpty(woAfterOffset))
                    {
                        if (!string.IsNullOrEmpty(m_filterText))
                        {
                            m_filterText += " AND ";
                        }
                        m_filterText += "WorkOrderDate > @StartDateAfter";
                        m_startDateAfter = Chrono.DateTimeAddOffset(Chrono.DayBeginningForDate(DateTime.Now), woAfterOffset);
                    }
                    if (!string.IsNullOrEmpty(m_filterText))
                    {
                        m_filterText = " WHERE " + m_filterText;
                    }
                }
            }
            public string FilterText
            {
                get { return m_filterText; }
                set { m_filterText = value; }
            }
            public DateTime StartDateBefore
            {
                get { return m_startDateBefore; }
                set { m_startDateBefore = value; }
            }
            public DateTime StartDateAfter
            {
                get { return m_startDateAfter; }
                set { m_startDateAfter = value; }
            }
        }
    }
}
