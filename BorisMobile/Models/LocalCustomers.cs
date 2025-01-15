using SQLite;

namespace BorisMobile.Models
{
    public class LocalCustomers
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public int CustomerId { get; set; }
        public int InheritFromCustomerId { get; set; }
        public string ContactName { get; set; }
        public string ContactXmlDocument { get; set; }
        public string XmlDoc { get; set; }

    }
}
