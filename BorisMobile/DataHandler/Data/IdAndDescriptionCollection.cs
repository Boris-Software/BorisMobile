using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;

namespace BorisMobile.DataHandler.Data
{
    public class IdAndDescriptionCollection : Collection<IdAndDescriptionListItem>
    {
        public IdAndDescriptionCollection(string[] textValues)
        {
            foreach (string oneValue in textValues)
            {
                this.Add(new IdAndDescriptionListItem(-1, oneValue));
            }
        }

        public IdAndDescriptionCollection()
            : base()
        {
        }
    }

    public class IdAndDescriptionListItem : IComparable, IGeneralIdOrGuidAndDescription
    {
        private int m_underlyingId;
        private string m_description;

        public IdAndDescriptionListItem(int underlyingId, string description)
        {
            m_underlyingId = underlyingId;
            m_description = description;
        }

        public int Id
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

        #region IComparable Members


        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            IdAndDescriptionListItem idAndDescListItem = obj as IdAndDescriptionListItem;
            if (idAndDescListItem != null)
            {
                return string.Compare(this.Description, idAndDescListItem.Description);
            }
            return 0;
        }
        #endregion

        public static int FindIndexOfId(IList items, string id)
        {
            return FindIndexOfId(items, id, false);
        }

        public static int FindIndexOfIdOrDescription(IList items, string id)
        {
            return FindIndexOfId(items, id, true);
        }

        public static int FindIndexOfId(IList items, string id, bool compareDescriptionToo)
        {
            // EPLR. Two items come from the back office with externalPKs with the same description.
            // This code would find a match based on description and select the wrong id, passing that back through XmlOutput etc
            // If we match on Id then that's great. If we match on description then don't return a match until we've checked all ids
            int descOnlyMatch = -1;
            string idLower = !string.IsNullOrEmpty(id) ? id.ToLower() : id;
            foreach (object item in items)
            {
                if (item is IdAndDescriptionListItem)
                {
                    if (((IdAndDescriptionListItem)item).Id.ToString() == id)
                    {
                        return items.IndexOf(item);
                    }
                    else if (compareDescriptionToo && ((IdAndDescriptionListItem)item).Description.ToLower() == idLower && descOnlyMatch == -1)
                    {
                        descOnlyMatch = items.IndexOf(item);
                    }
                }
                else if (item is GuidAndDescriptionListItem)
                {
                    if (((GuidAndDescriptionListItem)item).Guid.ToString("B") == id)
                    {
                        return items.IndexOf(item);
                    }
                    else if (compareDescriptionToo && ((GuidAndDescriptionListItem)item).Description.ToLower() == idLower && descOnlyMatch == -1)
                    {
                        descOnlyMatch = items.IndexOf(item);
                    }
                }
            }
            return descOnlyMatch;
        }

        public static int FindIndexOfDescription(IList items, string description)
        {
            string loweredDesc = description.ToLower();
            foreach (object item in items)
            {
                if (item is IdAndDescriptionListItem)
                {
                    if (((IdAndDescriptionListItem)item).Description.ToLower() == loweredDesc)
                    {
                        return items.IndexOf(item);
                    }
                }
                else if (item is GuidAndDescriptionListItem)
                {
                    if (((GuidAndDescriptionListItem)item).Description.ToLower() == loweredDesc)
                    {
                        return items.IndexOf(item);
                    }
                }
            }
            return -1;
        }

        public static int FindIndexOfDescriptionIgnoreWhiteSpace(IList items, string description)
        {
            // Deals with lower case anyway in CompareOptions
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            foreach (object item in items)
            {
                if (item is IdAndDescriptionListItem)
                {
                    if (compareInfo.Compare(((IdAndDescriptionListItem)item).Description, description, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                    {
                        return items.IndexOf(item);
                    }
                }
                else if (item is GuidAndDescriptionListItem)
                {
                    if (compareInfo.Compare(((GuidAndDescriptionListItem)item).Description, description, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                    {
                        return items.IndexOf(item);
                    }
                }
            }
            return -1;
        }

        public static int FindIndexOfId(IList items, int id)
        {
            foreach (object item in items)
            {
                if (item is IdAndDescriptionListItem)
                {
                    if (((IdAndDescriptionListItem)item).Id == id)
                    {
                        return items.IndexOf(item);
                    }
                }
            }
            return -1;
        }

        public static int FindListItemIdFromDescription(IList items, string description)
        {
            int itemIndex = FindIndexOfDescription(items, description);
            if (itemIndex != -1)
            {
                IdAndDescriptionListItem item = items[itemIndex] as IdAndDescriptionListItem;
                if (item != null)
                {
                    return item.Id;
                }
            }
            return -1;
        }

        public static void Sort(IdAndDescriptionCollection items)
        {
        }

        public object GetIdOrGuid()
        {
            return Id;
        }

        public string GetDescription()
        {
            return Description;
        }
    }
}
