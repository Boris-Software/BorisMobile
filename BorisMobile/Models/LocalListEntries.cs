using SQLite;

namespace BorisMobile.Models
{
    public class LocalListEntries
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public int ListId { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public int ListEntryId { get; set; }
        public string XmlDoc { get; set; }
        public int StatusId { get; set; }

    }
}
