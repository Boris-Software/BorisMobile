namespace BorisMobile.Models.DynamicFormModels
{
    public class SectionModel
    {
        public string Description { get; set; }
        public string UniqueName { get; set; }
        public bool IsRepeatable { get; set; }
        public bool AllowSectionDelete { get; set; }
        public List<ElementModel> Elements { get; set; }
    }
}
