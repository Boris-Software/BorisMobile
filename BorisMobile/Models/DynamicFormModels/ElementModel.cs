using System.ComponentModel;

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
        public string Value { get; set; }
        public int? Lines { get; set; }
        public int? PenWidth { get; set; }
        public bool? ArrangeHorizontally { get; set; }
        public bool? UseListItemImages { get; set; }
        public bool? ReportOnly { get; set; }

        public string? Condition0 { get; set; }

        public string? Condition1 { get; set; }
        public string? TextCol0 { get; set; }
        public string? EntityType0 { get; set; }
        public string? EntityType1 { get; set; }
        public int OutputField { get; set; }
        public string Calculation { get; set; }
        public int ResultType { get; set; }
        public string CurrencySymbol { get; set; }
        public string ExternalSystemField { get; set; }
        public bool GPSUse { get; set; }
        public bool NetworkUse { get; set; }

        public int? MinuteIncrement { get; set; }
        public bool CollectResultsAnyway { get; set; }


        public event EventHandler<string> ValueChanged;

        public void UpdateValue(string newValue)
        {
            Value = newValue;
            ValueChanged?.Invoke(this, newValue);
        }

        
    }
}
