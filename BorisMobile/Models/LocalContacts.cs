using SQLite;

namespace BorisMobile.Models
{
    public class LocalContacts
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public string XmlDoc { get; set; }
        public int LocationId { get; set; }
        public int ContactId { get; set; }

    }
}
