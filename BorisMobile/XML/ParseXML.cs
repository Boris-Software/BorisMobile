using System.Xml.Linq;
using System.Xml.Serialization;

namespace BorisMobile.XML
{
    public class ParseXML
    {

        public List<Item> ParseItems(string xml)
        {
            List<Item> itemL = new List<Item>();
            try
            {
                var doc = XDocument.Parse(xml);
                // Parse items from XML
                var items = (from item in doc.Descendants("Item")
                             select new Item
                             {
                                 Page = (int)item.Attribute("page"),
                                 NewPage = (int?)item.Attribute("newPage"),
                                 Name = (string)item.Element("Name"),
                                 Image = (string)item.Element("Image"),
                                 //NewImage = item.Element("Image").ToString().Replace(" ",string.Empty),
                                 ActionType = (string)item.Element("ActionType"),
                                 Additional = (string)item.Element("Additional"),
                                 Mode = (string)item.Attribute("mode"),
                                 TemplateId = (int?)item.Attribute("templateId"),
                                 TitleXPath = (string)item.Attribute("titleXPath"),
                                 Parameters = item.Elements("Parameter").Select(p => (string)p).ToList(),
                                 Template = item.Element("Template") != null
                                    ? new Template
                                    {
                                        Type = item.Element("Template").Attribute("type") != null
                                                ? (string)item.Element("Template").Attribute("type")
                                                : null,
                                        Entries = item.Element("Template").Elements("Entry")
                                                .Select(e => (int?)e.Attribute("id") ?? 0)
                                                .ToList()
                                    }
                                    : null
                             }).ToList();

                // Build hierarchy: first, find root items (main pages with page="0")
                //itemL = items.Where(i => i.Page == 0).ToList();
                itemL = items;

                // Map each main page to its children by matching NewPage values
                //foreach (var root in itemL)
                //{
                //    AddChildren(root, items);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return itemL;
        }

        public void AddChildren(Item parent, List<Item> items)
        {
            try
            {
                // Find items where page matches parent’s newPage
                var children = items.Where(i => i.Page == parent.NewPage).ToList();
                foreach (var child in children)
                {
                    parent.Children.Add(child);
                    // Recursively add any sub-items to this child
                    this.AddChildren(child, items);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public class Config
        {
            public Header Header { get; set; }
            public List<Item> Items { get; set; }
        }

        public class Header
        {
            public string ScreenTitle { get; set; }
            public string BackgroundColour { get; set; }
            public string TitleColour { get; set; }
        }

        [XmlRoot("Item")]
        public class Item
        {
            [XmlElement("Page")]
            public int Page { get; set; }
            [XmlElement("NewPage")]
            public int? NewPage { get; set; }
            [XmlElement("Name")]
            public string Name { get; set; }
            [XmlElement("Image")]
            public string Image { get; set; }
            [XmlElement("NewImage")]
            public string NewImage { get; set; }
            [XmlElement("ActionType")]
            public string ActionType { get; set; }
            [XmlArray("Parameters")]
            [XmlArrayItem("Parameter")]
            public List<string> Parameters { get; set; } = new List<string>();
            [XmlElement("Additional")]
            public string Additional { get; set; }
            [XmlElement("Template")]
            public Template Template { get; set; }
            [XmlElement("Mode")]
            public string Mode { get; set; }
            [XmlElement("TemplateId")]
            public int? TemplateId { get; set; }
            [XmlElement("TitleXPath")]
            public string TitleXPath { get; set; }

            // List of child items
            [XmlArray("Children")]
            [XmlArrayItem("Child")]
            public List<Item> Children { get; set; } = new List<Item>();
        }

        public class Template
        {
            [XmlElement("Type")]
            public string Type { get; set; }
            [XmlArray("Entries")]
            [XmlArrayItem("Entry")]
            public List<int> Entries { get; set; }
        }

        public class Entry
        {
            [XmlAttribute("id")]
            public int Id { get; set; }
        }
    }
}
