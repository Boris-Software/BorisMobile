namespace BorisMobile.Models.DynamicFormModels
{
    public class SectionModel
    {
        public string Description { get; set; }
        public string UniqueName { get; set; }
        public bool IsRepeatable { get; set; }
        public bool AllowSectionDelete { get; set; }
        public bool CollectResultsAnyway { get; set; }
        public bool? ReportOnly { get; set; }

        public string? Condition0 { get; set; }

        public string? Condition1 { get; set; }
        public List<ElementModel> Elements { get; set; }
        public List<RepeatableInstance> RepeatableInstances { get; set; } = new();

    }

    public class RepeatableInstance
    {
        public int Score { get; set; }
        public List<ElementModel> Elements { get; set; }
    }
}
