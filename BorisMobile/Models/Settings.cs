using SQLite;

namespace BorisMobile.Models
{
    public class Settings
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string KeyName { get; set; }
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public DateTime DateTimeValue { get; set; }
    }
}
