using BorisMobile.Models.DynamicFormModels;

namespace BorisMobile.Services.Interfaces
{
    public interface IXmlParserService
    {
        Task<FormConfigModel> ParseXmlConfiguration(string xmlContent);
        string GenerateXmlFromFormData(FormConfigModel formConfig);
    }
}
