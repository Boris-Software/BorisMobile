using BorisMobile.DataHandler;
using BorisMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Services
{
    public class InProgressService
    {
        InProgressDataHandler handler;
        public InProgressService()
        {
            handler = new InProgressDataHandler();
        }

        public async Task<List<InProgress>> GetInprogressListData(WorkOrderList workOrder)
        {
            return await handler.GetData(workOrder);
        }

        public async Task DeleteInProgress(InProgress progress)
        {
           await handler.DeleteInProgress(progress);
        }
    }
}
