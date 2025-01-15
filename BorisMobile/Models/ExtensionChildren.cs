using SQLite;

namespace BorisMobile.Models
{
    public class ExtensionChildren
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int ExtensionId { get; set; }
        public int ListEntryId { get; set; }
        public int Quantity { get; set; }
        public string DecimalValue { get; set; }

    }
}
