using SQLite;
using System.Reflection.Metadata;

namespace BorisMobile.Models
{
    public class Attachments
    {
        public Attachments()
        {

        }
        public Attachments(Guid IdGuid, string FileName)
        {
            this.IdGuid = IdGuid;
            this.FileName = FileName;
        }

        [PrimaryKey]
        public int Id { get; set; }
        public Guid IdGuid { get; set; }

        public string UniqueName {  get; set; }
        public Blob AttachmentData {  get; set; }

        public int Status {  get; set; }
        public int Repeat {  get; set; }

        private string fileName;
        public string FileName
        {
            get { return fileName; }

            set
            {
                fileName = value;
                DisplayName = fileName.Replace("_" + IdGuid, "");
            }
        }
        public Guid SubFormIdGuid {  get; set; }

        public int IsCopiedFromWorkOrder {  get; set; }
        public int PageRepeatListItemId { get; set; }
        public Guid PKGuid {  get; set; }
        public String DisplayName { get; set; }
                                                                                
    }
}
