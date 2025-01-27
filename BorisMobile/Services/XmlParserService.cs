using BorisMobile.Models.DynamicFormModels;
using BorisMobile.Services.Interfaces;
using System.Xml.Linq;
using System.Xml;
using static System.Collections.Specialized.BitVector32;

namespace BorisMobile.Services
{
    public class XmlParserService : IXmlParserService
    {
        public async Task<FormConfigModel> ParseXmlConfiguration(string xmlContent)
        {
            try
            {
                XDocument doc = XDocument.Parse(xmlContent);
                var configElement = doc.Descendants("Config").FirstOrDefault();

                if (configElement == null)
                    throw new XmlException("Invalid XML configuration");

                // Extract Document properties
                var documentElement = configElement.Element("Document");
                var formConfig = new FormConfigModel
                {
                    ScreenTitle = configElement.Element("Header")?.Element("ScreenTitle")?.Value ?? "Dynamic Form",
                    DocumentName = documentElement?.Attribute("name")?.Value,
                    DocumentDescription = documentElement?.Attribute("desc")?.Value,
                    //Pages = new List<PageModel>()
                };

                // Parse SubDocuments as Pages
                var subDocuments = configElement.Descendants("SubDocument");
                foreach (var subDoc in subDocuments)
                {
                    var subDocModel = new SubDocumentModel
                    {
                        Name = subDoc.Attribute("name")?.Value ?? "SubDocument",
                        Description = subDoc.Attribute("desc")?.Value,
                        IsMandatory = subDoc.Attribute("mandatory")?.Value == "yes",
                        UniqueName = subDoc.Attribute("uniquename")?.Value,
                        Pages = new List<PageModel>()
                    };

                    // Parse Pages
                    var pages = subDoc.Descendants("Page");
                    foreach (var page in pages)
                    {
                        var pageModel = new PageModel
                        {
                            Name = page.Attribute("name")?.Value ?? "Page",
                            //Description = subDoc.Attribute("desc")?.Value,
                            //IsMandatory = subDoc.Attribute("mandatory")?.Value == "yes",
                            UniqueName = page.Attribute("uniquename")?.Value,
                            Sections = new List<SectionModel>()
                        };

                        // Parse Sections
                        var sections = page.Descendants("Section");
                        foreach (var sectionElement in sections)
                        {
                            var section = new SectionModel
                            {
                                Description = sectionElement.Attribute("desc")?.Value ?? "Section",
                                UniqueName = sectionElement.Attribute("uniquename")?.Value,
                                IsRepeatable = sectionElement.Attribute("repeat")?.Value == "yes",
                                Elements = new List<ElementModel>()
                            };

                            // Parse different element types
                            ParseElements(sectionElement, section.Elements);

                            pageModel.Sections.Add(section);
                        }
                        subDocModel.Pages.Add(pageModel);
                    }

                    formConfig.SubDocumentModel = subDocModel;
                }

                return formConfig;
            }
            catch (Exception ex)
            {
                throw new XmlException("Error parsing XML configuration", ex);
            }
        }

        private void ParseElements(XElement sectionElement, List<ElementModel> elements)
        {
            var elementTypes = new Dictionary<string, Func<XElement, ElementModel>>
            {
                { "Combo", ParseComboBox },
                { "TextBox", ParseTextBox },
                { "Integer", ParseIntegerBox },
                { "Photo", ParsePhotoControl },
                { "Video", ParseVideoControl },
                { "Date", ParseDateControl },
                { "StaticText", ParseStaticText }
            };

            foreach (var element in sectionElement.Elements())
            {
                if (elementTypes.TryGetValue(element.Name.LocalName, out var parserMethod))
                {
                    var parsedElement = parserMethod(element);
                    if (parsedElement != null)
                        elements.Add(parsedElement);
                }
            }
        }

        private ElementModel ParseComboBox(XElement element)
        {
            return new ElementModel
            {
                Type = "Combo",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                ListId = element.Attribute("listid")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                DefaultValue = element.Attribute("default")?.Value
            };
        }

        private ElementModel ParseTextBox(XElement element)
        {
            var attribute = element.Attribute("lines");
            if (attribute != null)
            {
                Console.WriteLine($"Attribute lines' exists with value: {attribute.Value}");
            }
            else
            {
                Console.WriteLine($"Attribute lines does not exist.");
            }

            return new ElementModel
            {
                Type = "TextBox",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                DefaultValue = element.Attribute("default")?.Value,
                MaxLength = element.Attribute("maxlen")?.Value,
                Lines = attribute !=null ?  string.IsNullOrEmpty(element.Attribute("lines")?.Value.ToString()) ? 0 : Convert.ToInt32(element.Attribute("lines")?.Value.ToString()) : 0
            };
        }

        private ElementModel ParseIntegerBox(XElement element)
        {
            return new ElementModel
            {
                Type = "Integer",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                MinValue = element.Attribute("min_value")?.Value,
                MaxValue = element.Attribute("max_value")?.Value
            };
        }

        private ElementModel ParseDateControl(XElement element)
        {
            return new ElementModel
            {
                Type = "Date",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes"
            };

        }

        private ElementModel ParseVideoControl(XElement element)
        {
            return new ElementModel
            {
                Type = "Video",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes"
            };

        }
        private ElementModel ParsePhotoControl(XElement element)
        {
            return new ElementModel
            {
                Type = "Photo",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes"
            };
        }

        private ElementModel ParseStaticText(XElement element)
        {
            return new ElementModel
            {
                Type = "StaticText",
                Text = element.Attribute("static_text")?.Value
            };
        }

        public string GenerateXmlFromFormData(FormConfigModel formConfig)
        {
            return null;
            // XML generation logic
        }
    }
}
