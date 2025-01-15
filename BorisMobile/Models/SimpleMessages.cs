using SQLite;

namespace BorisMobile.Models
{
    public class SimpleMessages
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public string XmlDoc { get; set; }
        public DateTime Created { get; set; }
        public int UserId { get; set; }
        public int MessageType { get; set; }

    }
}
