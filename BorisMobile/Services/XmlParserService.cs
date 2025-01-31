using BorisMobile.Models.DynamicFormModels;
using BorisMobile.Services.Interfaces;
using Microsoft.Maui.Controls;
using System.Xml;
using System.Xml.Linq;

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
                                AllowSectionDelete = sectionElement.Attribute("allow_section_delete")?.Value == "yes",
                                ReportOnly = sectionElement.Attribute("report_only")?.Value == "yes",
                                Condition0 = sectionElement.Attribute("condition0")?.Value,
                                Condition1 = sectionElement.Attribute("condition1")?.Value,
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
                Console.WriteLine($"Error parsing XML configuration {ex}");
                return null;
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
                { "StaticText", ParseStaticText },
                { "Signature", ParseSignature },
                { "MultiChoice", ParseMultiChoice },
                { "OutputField", ParseOutputField },
                { "ActionButtons", ParseActionButtons},
                { "Score", ParseScore},
                { "GPSEarliest", ParseGPSEarliest},
                { "Time", ParseTime},
                { "GenericAttachments", ParseGenericAttachments},
                //{ "ExternalWorkOrderAttachments", ParseExternalWorkOrderAttachments},
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
                DefaultValue = element.Attribute("default")?.Value,
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };
        }

        private ElementModel ParseTextBox(XElement element)
        {
            var attribute = element.Attribute("lines");

            return new ElementModel
            {
                Type = "TextBox",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                DefaultValue = element.Attribute("default")?.Value,
                MaxLength = element.Attribute("maxlen")?.Value,
                Lines = attribute !=null ?  string.IsNullOrEmpty(element.Attribute("lines")?.Value.ToString()) ? 0 : Convert.ToInt32(element.Attribute("lines")?.Value.ToString()) : 0,
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };
        }

        private ElementModel ParseMultiChoice(XElement element)
        {
            return new ElementModel
            {
                Type = "MultiChoice",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };
        }

        
        private ElementModel ParseOutputField(XElement element)
        {
            return new ElementModel
            {
                Type = "OutputField",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };
        }

        private ElementModel ParseActionButtons(XElement element)
        {
            return new ElementModel
            {
                Type = "ActionButtons",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                TextCol0 = element.Attribute("text_col0")?.Value,
            };
        }
        private ElementModel ParseScore(XElement element)
        {
            return new ElementModel
            {
                Type = "Score",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                TextCol0 = element.Attribute("text_col0")?.Value,
                Calculation = element.Attribute("calculation")?.Value,
                ResultType = Convert.ToInt32(element.Attribute("resultType")?.Value),
                CurrencySymbol = element.Attribute("currencySymbol")?.Value,
            };
        }
        private ElementModel ParseGPSEarliest(XElement element)
        {
            return new ElementModel
            {
                Type = "GPSEarliest",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                GPSUse = element.Attribute("lm_gps_use")?.Value == "yes",
                NetworkUse = element.Attribute("lm_network_use")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                TextCol0 = element.Attribute("text_col0")?.Value,
                Calculation = element.Attribute("calculation")?.Value,
                ResultType = Convert.ToInt32(element.Attribute("resultType")?.Value),
                CurrencySymbol = element.Attribute("currencySymbol")?.Value,
            };
        }

        private ElementModel ParseGenericAttachments(XElement element)
        {
            return new ElementModel
            {
                Type = "GenericAttachments",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                EntityType0 = element.Attribute("entityType0")?.Value,
                EntityType1 = element.Attribute("entityType1")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                MinuteIncrement = Convert.ToInt32(element.Attribute("minuteIncrement")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                GPSUse = element.Attribute("lm_gps_use")?.Value == "yes",
                NetworkUse = element.Attribute("lm_network_use")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                TextCol0 = element.Attribute("text_col0")?.Value,
                Calculation = element.Attribute("calculation")?.Value,
                ResultType = Convert.ToInt32(element.Attribute("resultType")?.Value),
                CurrencySymbol = element.Attribute("currencySymbol")?.Value,
            };
        }
        private ElementModel ParseTime(XElement element)
        {
            return new ElementModel
            {
                Type = "Time",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                MinuteIncrement = Convert.ToInt32(element.Attribute("minuteIncrement")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                GPSUse = element.Attribute("lm_gps_use")?.Value == "yes",
                NetworkUse = element.Attribute("lm_network_use")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                TextCol0 = element.Attribute("text_col0")?.Value,
                Calculation = element.Attribute("calculation")?.Value,
                ResultType = Convert.ToInt32(element.Attribute("resultType")?.Value),
                CurrencySymbol = element.Attribute("currencySymbol")?.Value,
            };
        }

        private ElementModel ParseExternalWorkOrderAttachments(XElement element)
        {
            return new ElementModel
            {
                Type = "ExternalWorkOrderAttachments",
                Text = element.Attribute("text")?.Value,
                ListId = element.Attribute("listid")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                ExternalSystemField = element.Attribute("externalSystemField")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                PenWidth = Convert.ToInt32(element.Attribute("penWidth")?.Value),
                MinuteIncrement = Convert.ToInt32(element.Attribute("minuteIncrement")?.Value),
                OutputField = Convert.ToInt32(element.Attribute("outputField")?.Value),
                ArrangeHorizontally = element.Attribute("arrangeHorizontally")?.Value == "yes",
                UseListItemImages = element.Attribute("useListItemImages")?.Value == "yes",
                GPSUse = element.Attribute("lm_gps_use")?.Value == "yes",
                NetworkUse = element.Attribute("lm_network_use")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                TextCol0 = element.Attribute("text_col0")?.Value,
                Calculation = element.Attribute("calculation")?.Value,
                ResultType = Convert.ToInt32(element.Attribute("resultType")?.Value),
                CurrencySymbol = element.Attribute("currencySymbol")?.Value,
            };
        }

        private ElementModel ParseSignature(XElement element)
        {
            //Convert.ToInt32(element.Attribute("penWidth")?.Value)
            return new ElementModel
            {
                Type = "Signature",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
                PenWidth = string.IsNullOrEmpty(element.Attribute("penWidth")?.Value.ToString()) ? 0 : Convert.ToInt32(element.Attribute("penWidth")?.Value.ToString()) ,
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
                MaxValue = element.Attribute("max_value")?.Value,
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,

            };
        }

        private ElementModel ParseDateControl(XElement element)
        {
            return new ElementModel
            {
                Type = "Date",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };

        }

        private ElementModel ParseVideoControl(XElement element)
        {
            return new ElementModel
            {
                Type = "Video",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };

        }
        private ElementModel ParsePhotoControl(XElement element)
        {
            return new ElementModel
            {
                Type = "Photo",
                Text = element.Attribute("text")?.Value,
                UniqueName = element.Attribute("uniquename")?.Value,
                IsMandatory = element.Attribute("mandatory")?.Value == "yes",
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };
        }

        private ElementModel ParseStaticText(XElement element)
        {
            return new ElementModel
            {
                Type = "StaticText",
                Text = element.Attribute("static_text")?.Value,
                AllowCreateNew = element.Attribute("allowCreateNew")?.Value == "yes",
                ReportOnly = element.Attribute("report_only")?.Value == "yes",
                Condition0 = element.Attribute("condition0")?.Value,
                Condition1 = element.Attribute("condition1")?.Value,
            };
        }

        public string GenerateXmlFromFormData(FormConfigModel formConfig)
        {
            return null;
            // XML generation logic
        }
    }
}
