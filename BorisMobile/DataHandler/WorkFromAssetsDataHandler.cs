using BorisMobile.DataHandler.Data;
using BorisMobile.DataHandler.Helper;
using BorisMobile.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using static BorisMobile.DataHandler.Data.DataEnum;
using static System.Collections.Specialized.BitVector32;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class WorkFromAssetsDataHandler : CommonDataHandler
    {
        public WorkFromAssetsDataHandler() { }

        public async Task<List<WorkFromAssets>> GetAssetData(int drawingListId)
        {
            Collection<object> list = null ;
            int assetList = await AssetListForDrawingList("drawingAssetsListId", drawingListId); 
            if (assetList > 0)
            {
                 list = await SetupAssetList(assetList, false, false, null);
            }

            List<WorkFromAssets> outputList = new List<WorkFromAssets>();
            foreach (var item in list)
            {
                WorkFromAssets assets = new WorkFromAssets();
                assets.Description = item.GetType().GetProperty("Description").GetValue(item, null).ToString();
                assets.Id = Convert.ToInt32(item.GetType().GetProperty("Id").GetValue(item, null));

                assets.genericList = GetGenericListData(assets.Id);
                outputList.Add(assets);

            }
            return outputList;
        }

        private async Task<int> AssetListForDrawingList(string assetsListIdAttName, int drawingListId)
        {
            return await GetAttributeForListEntry_int(drawingListId, /* "drawingAssetsListId" */ assetsListIdAttName, -1); // New style loading of icons from other forms
        }
        public async Task<int> GetAttributeForListEntry_int(int listEntryId, string attName, int defaultValue)
        {
            string stringValue = await GetAttributeForListEntry(listEntryId, attName);
            return DataHandlerHelper.IntFromString(stringValue, defaultValue);
        }
        private async Task<Collection<object>> SetupAssetList( int assetListId, bool allowLocalDelete, bool hideTotal, string filterCondition)
        {
            
            Collection<object> fullAndLocalItems = await GetListEntriesAndLocalListEntriesForList(assetListId, null);
            return fullAndLocalItems;
        }

        public async Task<Collection<object>> GetListEntriesAndLocalListEntriesForList(int listId, RecordFilterFunction filterFunction)
        {
            Collection<object> full = new Collection<object>();
            IdAndDescriptionCollection lis = await GetListEntriesForList(listId, null, filterFunction);
            foreach (IdAndDescriptionListItem one in lis)
            {
                full.Add(one);
            }
            Collection<GuidAndDescriptionListItem> locals = GetLocalListEntriesForList(listId, filterFunction);
            foreach (GuidAndDescriptionListItem local in locals)
            {
                full.Add(local);
            }
            return full;
        }

        public GenericLists GetGenericListData(int id)
        {
            GenericLists genericListData = new GenericLists();
            SqlCeCommand command = GetCommandObject("select * from GenericLists where Id = " + id);
            if (command != null)
            {
                SqlCeDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    genericListData.Id = Convert.ToInt32(reader["Id"]);
                    genericListData.Desc = reader["Desc"].ToString();
                    genericListData.Seq = Convert.ToInt32(reader["Seq"]);
                    genericListData.List = Convert.ToInt32(reader["List"]);
                    genericListData.Score = Convert.ToInt32(reader["Score"]);
                    genericListData.XmlDoc =  reader["XmlDoc"].ToString();
                }
            }
            return genericListData;
        }
        public Collection<GuidAndDescriptionListItem> GetLocalListEntriesForList(int listId, RecordFilterFunction filterFunction)
        {
            SqlCeCommand command = GetCommandObject("SELECT IdGuid, Description FROM LocalListEntries WHERE ListId = " + listId + " AND ListEntryId IS NULL AND (StatusId is NULL OR StatusId != "+ (int)LocalEntityStatusEnum.DELETED + ") ORDER BY Description");
            //m_dataHandler.AddIntParam(command, "deletedId", (int)LocalEntityStatusEnum.DELETED);
            Collection<GuidAndDescriptionListItem> listEntries = new Collection<GuidAndDescriptionListItem>();
            if (command != null)
            {
                SqlCeDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GuidAndDescriptionListItem listData = new GuidAndDescriptionListItem((Guid)reader["IdGuid"], reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString());
                    //if (!string.IsNullOrEmpty(activityId))
                    //{
                    //    string listEntryGuid = GetAttributeForLocalListEntry(listData.Guid, "activityId");
                    //    if (!string.IsNullOrEmpty(listEntryGuid) && listEntryGuid == activityId)
                    //        listEntries.Add(listData);
                    //}
                    //else
                    //{
                    //    listEntries.Add(listData);
                    //}
                    if (filterFunction != null && !filterFunction(reader))
                    {
                        continue;
                    }
                    listEntries.Add(listData);
                }
                reader.Close();
                command.Dispose();
            }
            return listEntries;
        }
    }
    }
