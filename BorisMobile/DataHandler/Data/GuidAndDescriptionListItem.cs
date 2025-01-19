using System.Collections;

namespace BorisMobile.DataHandler.Data
{
    public class GuidAndDescriptionListItem : IGeneralIdOrGuidAndDescription
    {
        private Guid m_underlyingId;
        private string m_description;

        public GuidAndDescriptionListItem(Guid underlyingId, string description)
        {
            m_underlyingId = underlyingId;
            m_description = description;
        }

        public Guid Guid
        {
            get
            {
                return m_underlyingId;
            }
        }

        public string Description
        {
            get
            {
                return m_description;
            }
        }

        public override string ToString()
        {
            return m_description;
        }

        public static int FindIndexOfId(IList items, Guid id)
        {
            foreach (object item in items)
            {
                if (item is GuidAndDescriptionListItem)
                {
                    if (((GuidAndDescriptionListItem)item).Guid == id)
                    {
                        return items.IndexOf(item);
                    }
                }
            }
            return -1;
        }

        public object GetIdOrGuid()
        {
            return Guid;
        }

        public string GetDescription()
        {
            return Description;
        }
    }
}
