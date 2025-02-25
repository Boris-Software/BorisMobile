using BorisMobile.Models.DynamicFormModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BorisMobile.Services
{
    public static class FormValidationExtensions
    {
        public static bool ValidateForm(FormConfigModel formModel)
        {
            foreach (var page in formModel.SubDocumentModel.Pages)
            {
                foreach (var section in page.Sections)
                {
                    foreach (var element in section.Elements)
                    {
                        if (element.Condition0 == null || element.Condition0.Equals(""))
                        {
                            if (element.IsMandatory && string.IsNullOrWhiteSpace(element.Value))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!element.Condition0.Equals("resultValueX;dummy;equals;dummy") || !element.Condition0.Equals("resultValue;dummy;equals;dummy"))
                            {
                                if (element.IsMandatory && string.IsNullOrWhiteSpace(element.Value))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }


        public static string SerializeToXml(FormConfigModel formData)
        {
            try
            {
                var root = new XElement("results");
                var subDoc = formData.SubDocumentModel;
                //foreach (var subDoc in subDocuments)
                //{
                var subDocElement = new XElement(subDoc.UniqueName);

                foreach (var page in subDoc.Pages)
                {
                    var pageElement = new XElement(page.UniqueName);

                    foreach (var section in page.Sections)
                    {
                        XElement sectionElement;

                        if (section.Condition0 != null && !section.Condition0.Equals(""))
                        {
                            sectionElement = new XElement(section.UniqueName);
                            if (section.CollectResultsAnyway)
                            {
                                foreach (var value in section.Elements)
                                {
                                    XElement valueElement=null;
                                    if (value.Type != "StaticText")
                                    {
                                        valueElement = new XElement(value.UniqueName,
                                        new XAttribute("res", value.Value == null ? "" : value.Value));
                                    }
                                    sectionElement.Add(valueElement);
                                }
                            }
                            else
                            {
                                sectionElement = new XElement(section.UniqueName,
                                    new XAttribute("hiding", "yes"));
                            }
                            pageElement.Add(sectionElement);
                        }
                        else
                        {
                            sectionElement = new XElement(section.UniqueName);
                            foreach (var value in section.Elements)
                            {
                                if (value.Type != "StaticText" && section.UniqueName != "calcs")
                                {
                                    var valueElement = new XElement(value.UniqueName,
                                    new XAttribute("res", value.Value == null ? "" : value.Value));

                                    if (value.AllowCreateNew)
                                    {
                                        valueElement.Add(new XAttribute("score", "0"));
                                    }
                                    if (section.IsRepeatable)
                                    {
                                        valueElement.Add(new XAttribute("count", "0"));
                                    }

                                    if (value.Condition0 != null && !value.Condition0.Equals(""))
                                    {
                                        if (value.Value == null)
                                        {
                                            if (value.CollectResultsAnyway)
                                            {
                                                if (value.Text.Equals("Date completed")){
                                                    valueElement = new XElement(value.UniqueName,
                                                       new XAttribute("res", value.Value == null ? DateTime.Now: value.Value));
                                                }
                                                else
                                                {
                                                    valueElement = new XElement(value.UniqueName,
                                                       new XAttribute("res", ""));
                                                }
                                            }
                                            else
                                            {
                                                valueElement.Add(new XAttribute("hiding", "yes"));
                                            }
                                        }  
                                    }
                                    sectionElement.Add(valueElement);
                                }
                            }
                            pageElement.Add(sectionElement);

                            if(section.RepeatableInstances!=null && section.RepeatableInstances.Count > 0)
                            {
                                int i = 1;
                                foreach (var repeatInstance in section.RepeatableInstances)
                                {
                                    sectionElement = new XElement(section.UniqueName);
                                    foreach (var value in repeatInstance.Elements)
                                    {
                                        if (value.Type != "StaticText" && section.UniqueName != "calcs")
                                        {
                                            var valueElement = new XElement(value.UniqueName,
                                            new XAttribute("res", value.Value == null ? "" : value.Value));

                                            if (value.AllowCreateNew)
                                            {
                                                valueElement.Add(new XAttribute("score", "0"));
                                            }
                                            if (section.IsRepeatable)
                                            {
                                                valueElement.Add(new XAttribute("count", i.ToString()));
                                            }

                                            if (value.Condition0 != null && !value.Condition0.Equals(""))
                                            {
                                                if (value.Value == null)
                                                {
                                                    if (value.CollectResultsAnyway)
                                                    {
                                                        if (value.Text.Equals("Date completed"))
                                                        {
                                                            valueElement = new XElement(value.UniqueName,
                                                               new XAttribute("res", value.Value == null ? DateTime.Now : value.Value));
                                                        }
                                                        else
                                                        {
                                                            valueElement = new XElement(value.UniqueName,
                                                               new XAttribute("res", ""));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        valueElement.Add(new XAttribute("hiding", "yes"));
                                                    }
                                                }
                                            }
                                            sectionElement.Add(valueElement);
                                        }
                                    }
                                    i++;
                                    pageElement.Add(sectionElement);
                                }
                            }
                        }

                        
                    }

                    subDocElement.Add(pageElement);
                }

                root.Add(subDocElement);
                //}

                return root.ToString(SaveOptions.DisableFormatting);
            }
            catch(Exception EX)
            {
                Console.WriteLine(EX.StackTrace);
                return "";
            }
        }

        public static DynamicFormData DeserializeFromXml(string xmlData)
        {
            var serializer = new XmlSerializer(typeof(DynamicFormData));
            using (var stringReader = new StringReader(xmlData))
            {
                return (DynamicFormData)serializer.Deserialize(stringReader);
            }
        }
    }
}
