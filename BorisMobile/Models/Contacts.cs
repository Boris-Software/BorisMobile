using SQLite;

namespace BorisMobile.Models
{
    public class Contacts
    {

        [PrimaryKey]
        public int Id { get; set; }
        public int Cust { get; set; }
        public int Loc { get; set; }
        public string Desc { get; set; }
        public string XmlDoc { get; set; }
    }
}
