using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Models
{
    public class InProgress
    {
        public Guid InProgressIdGuid {  get; set; }
        public string XmlDocument {  get; set; }
        public DateTime DateTimeStarted {  get; set; }
        public string AuditDesc {  get; set; }
        public int TemplateId {  get; set; }
        public string SupplementaryXml {  get; set; }
        public bool IsFromServer {  get; set; }
    }
}
