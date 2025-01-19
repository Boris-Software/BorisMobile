using BorisMobile.DataHandler;
using BorisMobile.DataHandler.Data;
using BorisMobile.Models;

namespace BorisMobile.Services
{

    public class WorkFromDrawingsService
    {
        WorkFromDrawingsDataHandler dataHandler;
        
        public WorkFromDrawingsService()
        {
            dataHandler = new WorkFromDrawingsDataHandler();
        }

        public async Task<IdAndDescriptionCollection> GetData(WorkOrderList SelectedItem )
        {
            return await dataHandler.GetInitData(SelectedItem);
        }
        
    }

}
