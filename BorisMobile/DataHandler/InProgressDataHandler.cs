using BorisMobile.Models;
using System;
using static SQLite.SQLite3;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class InProgressDataHandler : CommonDataHandler
    {
        public InProgressDataHandler()
        {
        }

        public async Task DeleteInProgress(InProgress inProgress)
        {
            try
            {
                //int rowsAffected = -1;
                using (var deleteCommand = GetCommandObject("Delete from AuditsInProgress Where IdGuid = '"+inProgress.InProgressIdGuid.ToString().ToUpper()+"'"))
                {
                    deleteCommand.ExecuteNonQuery();

                    //if (rowsAffected > 0)
                    //{
                        using (var deleteAttachmentCommand = GetCommandObject("Delete from Attachments Where IdGuid ='"+inProgress.InProgressIdGuid.ToString().ToUpper() + "'"))
                        {
                            deleteAttachmentCommand.ExecuteNonQuery();
                            //if (rowsAffectedAttachment > 0)
                            //{
                             //   return rowsAffected + rowsAffectedAttachment;
                            //}
                        }
                    }
                
                //return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //return -1;
            }
        }

        public async Task<List<InProgress>> GetData(WorkOrderList workOrder)
        {
            List<InProgress> list = new List<InProgress>();
            SqlCeCommand command = GetCommandObject(string.Format(@"SELECT 
                      AuditsInProgress.IdGuid, XmlResults as XmlDocument, DateTimeStarted, Audits.Desc AS Audit, Audits.Id as TemplateId, SupplementaryXmlDocument as SupplementaryXml, IsFromServer
                        FROM         AuditsInProgress INNER JOIN
                                              Audits ON AuditsInProgress.AuditId = Audits.Id
                        WHERE     AuditsInProgress.WorkOrderId = {0}
                        AND (IsReadOnly IS NULL OR (IsReadOnly IS NOT NULL AND IsReadOnly <> 1))
                        ORDER BY DateTimeStarted", workOrder.workOrder.Id));
            command.Prepare();
            //return command;

            SqlCeDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                InProgress inProgress = new InProgress();
                
                inProgress.XmlDocument = reader["XmlDocument"] == DBNull.Value ? "" : reader["XmlDocument"].ToString();
                inProgress.InProgressIdGuid =  Guid.Parse(reader["IdGuid"].ToString()); 
                inProgress.DateTimeStarted = reader["DateTimeStarted"] == DBNull.Value ? new DateTime() : DateTime.Parse(reader["DateTimeStarted"].ToString());
                inProgress.AuditDesc = reader["Audit"] == DBNull.Value ? "" : reader["Audit"].ToString();
                inProgress.TemplateId = reader["TemplateId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TemplateId"].ToString());
                inProgress.SupplementaryXml = reader["SupplementaryXml"] == DBNull.Value ? "" : reader["SupplementaryXml"].ToString();
                inProgress.IsFromServer = reader["IsFromServer"] != DBNull.Value && Convert.ToInt32(reader["IsFromServer"]) == 1;

                list.Add(inProgress);
            }

            return list;
        }

        public async Task UpdateAuditInProgress(AuditsInProgress progress)
        {
            try
            {
                using (var updateCommand = GetCommandObject("Update AuditsInProgress SET XmlResults = '"+progress.XmlResults+"' Where IdGuid = '" + progress.IdGuid.ToString().ToUpper() + "'"))
                {
                    updateCommand.ExecuteNonQuery();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
