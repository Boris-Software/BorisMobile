using BorisMobile.Models;
using BorisMobile.Utilities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class DocumentsHandler : CommonDataHandler
    {

        public DocumentsHandler() { }

        public Collection<Attachments> GetGenericAttachmentsForEntity(string entityType, int entityId)
        {
            string sql = string.Format("SELECT IdGuid, ShortFileName FROM GenericAttachments WHERE NeedToDownload = 0 AND EntityType = '{0}'", entityType);
            if (entityId != -1)
            {
                sql += " AND EntityId = " + entityId;
            }
            return GetAttachmentsFromSQL(sql);
        }

        private Collection<Attachments> GetAttachmentsFromSQL(string sql)
        {
            //InfoLog("GetAtts: " + sql);
            Collection<Attachments> attachments = new Collection<Attachments>();
            using (SqlCeCommand command = GetCommandObject(sql))
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

        
    }
}
