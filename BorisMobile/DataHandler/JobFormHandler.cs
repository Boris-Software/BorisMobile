using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using Microsoft.Maui.Controls;
using System.Xml;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class JobFormHandler : CommonDataHandler
    {

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
