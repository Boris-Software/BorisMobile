using BorisMobile.Models.DynamicFormModels;

namespace BorisMobile.Services.Interfaces
{
    public interface IFormGenerationService
    {
        Task<Page> CreateDynamicForm(FormConfigModel formConfig);

    }
}
