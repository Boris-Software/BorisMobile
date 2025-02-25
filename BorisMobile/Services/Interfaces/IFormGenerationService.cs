using BorisMobile.Models;
using BorisMobile.Models.DynamicFormModels;

namespace BorisMobile.Services.Interfaces
{
    public interface IFormGenerationService
    {
        Task<Page> CreateDynamicForm(FormConfigModel formConfig,WorkOrderList workOrer, AuditsInProgress inProgress);

    }
}
