using BorisMobile.DataHandler;
using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using System.Collections.ObjectModel;

namespace BorisMobile.Services
{
    public class WorkFromAssetsService
    {
        WorkFromAssetsDataHandler dataHandler;
        public WorkFromAssetsService()
        {
            dataHandler = new WorkFromAssetsDataHandler();
        }

        public async Task<List<WorkFromAssets>> GetData(WorkOrderList SelectedItem,int drawingListId)
        {
            var list = await dataHandler.GetAssetData(drawingListId);
            return list;
        }


        
    }
}
