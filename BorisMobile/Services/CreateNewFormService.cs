using BorisMobile.DataHandler;
using BorisMobile.DataHandler.Data;
using BorisMobile.Models;

namespace BorisMobile.Services
{
    public class CreateNewFormService
    {
        CreateNewFormHandler handler;
        public CreateNewFormService() 
        {
            handler = new CreateNewFormHandler();
        }

        public async Task<List<TemplateDocument>> GetCreateFormListData(WorkOrderList workOrder)
        {
            return await handler.GetData(workOrder);
        }
    }
}
