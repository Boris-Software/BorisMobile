using BorisMobile.DataHandler.Data;
using BorisMobile.DataHandler.Helper;
using BorisMobile.Models;
using System.Xml;
using static BorisMobile.DataHandler.Data.DataEnum;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class WorkFromDrawingsDataHandler : CommonDataHandler
    {
        WorkOrderList workOrderList;

        public WorkFromDrawingsDataHandler() 
        {
        }
        public async Task<IdAndDescriptionCollection> GetInitData(WorkOrderList workOrder)
        {
            try
            {
                workOrderList = workOrder;
                ListEntry woPageOptions = GetListEntry(Convert.ToInt32(workOrder.workOrder.DefinitionId));
                XmlDocument additionalSettings = AdditionalSettingsHelper.AdditionalSettings(woPageOptions);

                int liveDrawingTemplateId = await GetAttributeForWODefDoc_int(Convert.ToInt32(workOrder.workOrder.DefinitionId), "liveDrawingTemplate", -1);

                if (liveDrawingTemplateId > 0)
                {
                    IdAndDescriptionCollection drawingListEntries = await DrawingListEntries(workOrder, additionalSettings);
                    return drawingListEntries;
                }
                else
                {
                    return null;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

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

        

        private async Task<int> GeneralEntityList(WorkOrderList workOrder, string entity, string attribute)
        {
            // Attribute could be hard-coded
            if (string.IsNullOrEmpty(attribute)) // check against errors in set up
            {
                return -1;
            }
            int attInt = DataHandlerHelper.IntFromString(attribute, -1);
            if (attInt > 0)
            {
                return attInt;
            }
            if (entity == "LO")
            {
                return await GetAttributeForLocation_int(GetLocationId(), attribute, -1);
            }
            else if (entity == "CU")
            {
                return await GetAttributeForCustomer_int(GetCustomerId(), attribute, -1);
            }
            else if (entity == "US")
            {
                return await GetAttributeForUser_int(Convert.ToInt32(Preferences.Get("UserId", 0)), attribute, -1);
            }
            return -1;
        }

        public async Task<int> GetAttributeForUser_int(int userId, string attName, int defaultValue)
        {
            string stringValue = await GetAttributeForUser(userId, attName);
            return DataHandlerHelper.IntFromString(stringValue, defaultValue);
        }
        public async Task<string> GetAttributeForUser(int userId, string attName)
        {
            return await GetAttributeForUser(userId, attName);
        }
        private int GetCustomerId()
        {
            if (workOrderList != null)
            {
                return workOrderList.workOrder.CustomerId;
            }
            return -1;
        }
        public async Task<int> GetAttributeForCustomer_int(int customerId, string attName, int defaultValue)
        {
            string stringValue = await GetAttributeForCustomer(customerId, attName);
            return DataHandlerHelper.IntFromString(stringValue, defaultValue);
        }
        public async Task<string> GetAttributeForCustomer(int customerId, string attName)
        {
            return await GetAttributeForCustomerID(customerId, attName);
        }
        private int GetLocationId()
        {
            if(workOrderList != null)
            {
                return workOrderList.workOrder.LocationId;
            }
            return -1;
        }

        public async Task<int> GetAttributeForLocation_int(int locationId, string attName, int defaultValue)
        {
            string stringValue = await GetAttributeForLocation(locationId, attName);
            return DataHandlerHelper.IntFromString(stringValue, defaultValue);
        }
        public async Task<string> GetAttributeForLocation(int locationId, string attName)
        {
            return await GetAttributeForLocationID(locationId, attName);
        }
        private async Task<IdAndDescriptionCollection> DrawingListEntries(WorkOrderList workOrder, XmlDocument additionalSettings)
        {
            if (workOrder != null)
            {
                int drawingListToUse = await DrawingListToUse(workOrder, additionalSettings);
                if (drawingListToUse > 0)
                {
                    return GetListEntriesForList(drawingListToUse);
                }
            }
            return null;
        }

        private async Task<int> DrawingListToUse(WorkOrderList workOrder, XmlDocument additionalSettings)
        {
            string entity = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "DrawingListEntity", "LO");
            string attribute = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "DrawingListAttribute", "drawingListId");
            return await GeneralEntityList(workOrder, entity, attribute);
        }

        public IdAndDescriptionCollection GetListEntriesForList(int listId)
        {
            return GetListEntriesForList(listId, null, null);
        }

        

        public void AddIntParam(SqlCeCommand command, string name, int value)
        {
            command.Parameters.Add(name, Microsoft.Data.Sqlite.SqliteType.Integer, 4);

        }

        

        
        

        

        

        public async Task<int> GetAttributeForWODefDoc_int(int workOrderDefinitionId, string attName, int defaultValue)
        {
            string strVal = await GetAttributeForWODefDoc(workOrderDefinitionId, attName);
            if (!string.IsNullOrEmpty(strVal))
            {
                if (!string.IsNullOrEmpty(strVal))
                {
                    int intValue;
                    if (Int32.TryParse(strVal, out intValue))
                    {
                        return intValue;
                    }
                }
            }
            return defaultValue;
        }

        public async Task<string> GetAttributeForWODefDoc(int workOrderDefinitionId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM WorkOrderDefinitions WHERE Id = " + workOrderDefinitionId, attName);
        }

        
    }
}
