using BorisMobile.DataHandler.Data;
using BorisMobile.DataHandler.Helper;
using BorisMobile.Models;
using Microsoft.Maui.Controls;
using System.Xml;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class JobFormHandler : CommonDataHandler
    {

        //public async Task<List<GenericListDefinitions>> GetLists(string fieldId)
        //{
        //    //DataEntryForm owningForm = (DataEntryForm)this.ConfiguredForm;
        //    XmlNodeList attachmentList = ExternalWorkOrderAttachmentsForWorkOrderItem(fieldId);
        //    if (attachmentList != null)
        //    {
        //        foreach (XmlNode attachmentDef in attachmentList)
        //        {
        //            XmlElement attachmentDefElement = (XmlElement)attachmentDef;
        //            string friendlyName = attachmentDefElement.GetAttribute("friendlyName");
        //            string attGuid = attachmentDefElement.GetAttribute("allocatedGuid");
        //            string buttonText = friendlyName.Replace("_" + attGuid, "");
        //            if (buttonText == "job.pdf" && BoolAtt("suppressDefaultJobPDF", false))
        //            {
        //                continue;
        //            }
        //            //Button button = AddButton(buttonText, this.Button_Click, ParentControl, true);
        //            //button.Name = friendlyName; // !!!!!!! ugh!!
        //            //m_buttons.Add(button);

        //            //m_guidXRef.Add(attGuid);
        //            //if (resultToSet != null)
        //            //{
        //            //    XmlElement resultElement = resultToSet.SelectSingleNode("button/@docGuid=" + attGuid) as XmlElement;
        //            //    if (resultElement != null)
        //            //    {
        //            //        SetTag(button, resultElement);
        //            //    }
        //            //}
        //        }
        //    }
        //}

        //public bool BoolAtt(string attName, bool defaultValue)
        //{
        //    return XML.XmlUtils.BoolAtt(m_xmlChars, attName, defaultValue);
        //}

        //public XmlNodeList ExternalWorkOrderAttachmentsForWorkOrderItem(string fieldId)
        //{
        //    XmlNodeList attachmentList = null;
        //    if (fieldId != "$$deviceWorkflow")
        //    {
        //        int delimIndex = fieldId.IndexOf("$");
        //        string uniqueName = fieldId.Substring(delimIndex + 1);
        //        if (WorkOrderDoc != null && uniqueName != "-1")
        //        {
        //            attachmentList = WorkOrderDoc.SelectNodes("//WorkOrderItems/" + uniqueName + "/fileName");
        //        }
        //    }
        //    return attachmentList;
        //}
        public async Task<string> GetOutputFielddata(int outfieldId, int locationId = 0, int customerId=0, int userId = 0 )
        {
            var fieldType = await GetStringFromDataSql("select Desc from GenericLists where Id = "+ outfieldId);

            switch (fieldType)
            {
                case "Location":
                    return await GetStringFromDataSql("SELECT Desc FROM Locations WHERE Id = " + locationId);
                    break;
                default:
                    return "";
            }

        }

        public async Task<List<GenericLists>> GetComboBoxData(int listId)
        {
            try
            {
                string query = "select * from GenericLists where List = " + listId + " order by Seq";
                //Id,Desc,Seq,List,Score,XmlDoc
                List<GenericLists> genericList = new List<GenericLists>();
                using (SqlCeCommand listEntryCommand = GetCommandObject(query))
                {
                    using (SqlCeDataReader reader = listEntryCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GenericLists genericLists = new GenericLists();
                            string desc = reader["Desc"] == DBNull.Value ? "" : reader["Desc"].ToString();
                            genericLists.Desc = desc;
                            genericLists.Id = Convert.ToInt32(reader["Id"].ToString());
                            genericLists.Seq = Convert.ToInt32(reader["Seq"].ToString());
                            genericLists.List = Convert.ToInt32(reader["List"].ToString());
                            genericLists.Score = Convert.ToInt32(reader["Score"].ToString());
                            genericLists.XmlDoc = reader["XmlDoc"] == DBNull.Value ? "" : reader["XmlDoc"].ToString();
                            genericList.Add(genericLists);
                        }
                    }
                }
                return genericList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
