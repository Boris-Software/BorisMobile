using SQLite;

namespace BorisMobile.Models
{
    public class WorkOrders
    {
        
        [PrimaryKey]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int LocationId { get; set; }
        public int TemplateId { get; set; }
        public DateTime WorkOrderDate { get; set; }
        public string XmlDoc { get; set; }
        public string? CustomOrder { get; set; }
        public string? CustomData { get; set; }
        public string? SupplementaryXmlDocument { get; set; }
        public int IsInitialised { get; set; }
        public string? GeneratingXmlDocument { get; set; }
        public string? OriginalCustomOrder { get; set; }
        public int GroupId { get; set; }
        public int AppStatusId { get; set; }
        public int UserId { get; set; }

        public string? Notes { get; set; }
        public string? JobNo { get; set; }
    }
}
