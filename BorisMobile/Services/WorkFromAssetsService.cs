using BorisMobile.DataHandler;
using BorisMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Services
{
    public class WorkFromAssetsService
    {
        WorkFromAssetsDataHandler dataHandler;
        public WorkFromAssetsService()
        {
            dataHandler = new WorkFromAssetsDataHandler();
        }

        public async void GetData(WorkOrderList SelectedItem)
        {
            //return await dataHandler.GetInitData(SelectedItem);
        }
    }
}
