using SQLite;

namespace BorisMobile.Models
{
    public class Users
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public int FilterListEntryId { get; set; }
        public string XmlDoc { get; set; }
    }
}
