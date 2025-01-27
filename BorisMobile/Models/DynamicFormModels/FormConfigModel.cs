namespace BorisMobile.Models.DynamicFormModels
{
    public class FormConfigModel
    {
        public string ScreenTitle { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDescription { get; set; }
        public SubDocumentModel SubDocumentModel { get; set; }
    }
}
