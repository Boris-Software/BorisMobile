using System.Xml.Serialization;

namespace BorisMobile.Models.DynamicFormModels
{
    [Serializable]
    public class DynamicFormData
    {
        [XmlElement("PageData")]
        public List<PageData> Pages { get; set; } = new List<PageData>();

        [XmlAttribute("FormId")]
        public string FormId { get; set; }

        [XmlAttribute("CreatedDate")]
        public DateTime CreatedDate { get; set; }
    }

    [Serializable]
    public class PageData
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("SectionData")]
        public List<SectionData> Sections { get; set; } = new List<SectionData>();
    }

    [Serializable]
    public class SectionData
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("ElementData")]
        public List<ElementData> Elements { get; set; } = new List<ElementData>();

        [XmlAttribute("IsRepeatable")]
        public bool IsRepeatable { get; set; }
    }

    [Serializable]
    public class ElementData
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlElement("Value")]
        public string Value { get; set; }

        [XmlAttribute("IsMandatory")]
        public bool IsMandatory { get; set; }
    }
}
