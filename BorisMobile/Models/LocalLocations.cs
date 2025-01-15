using SQLite;

namespace BorisMobile.Models
{
    public class LocalLocations
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public int CustomerId { get; set; }
        public Guid CustomerIdGuid { get; set; }
        public string Description { get; set; }
        public Guid DateTimeCreated { get; set; }
        public int LocationId { get; set; }
        public string ContactName { get; set; }
        public string ContactXmlDocument { get; set; }
        public string XmlDoc { get; set; }

    }
}
