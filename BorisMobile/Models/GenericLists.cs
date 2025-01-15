using SQLite;

namespace BorisMobile.Models
{
    public class GenericLists
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Desc { get; set; }
        public int Seq { get; set; }
        public int List { get; set; }
        public int Score { get; set; }
        public string XmlDoc { get; set; }
    }
}
