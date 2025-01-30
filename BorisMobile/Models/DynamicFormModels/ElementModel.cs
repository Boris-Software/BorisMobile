namespace BorisMobile.Models.DynamicFormModels
{
    public class ElementModel
    {
        public string UniqueName { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string DefaultValue { get; set; }
        public string ListId { get; set; }
        public string MaxLength { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public bool IsMandatory { get; set; }
        public bool AllowCreateNew { get; set; }
        public object Value { get; set; }
        public int? Lines { get; set; }
        public int? PenWidth { get; set; }
        public bool? ArrangeHorizontally { get; set; }
        public bool? UseListItemImages { get; set; }
    }
}
