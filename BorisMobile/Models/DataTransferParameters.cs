using SQLite;

namespace BorisMobile.Models
{
    public class DataTransferParameters
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public string EntityType { get; set; }
        public DateTime LastTransactionDate { get; set; }
        public int LastId { get; set; }
        public Guid LastGuid { get; set; }
        public DateTime BaseTransactionDate { get; set; }
    }
}
