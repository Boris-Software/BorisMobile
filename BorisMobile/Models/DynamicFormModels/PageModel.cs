namespace BorisMobile.Models.DynamicFormModels
{
    public class PageModel
    {
        public string Name { get; set; }
        //public string Description { get; set; }
        //public bool IsMandatory { get; set; }
        public string UniqueName { get; set; }
        public List<SectionModel> Sections { get; set; }
    }
}
