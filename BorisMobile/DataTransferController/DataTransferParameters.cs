namespace BorisMobile.DataTransferController
{
    public class DataTransferParameters
    {
        private string m_entityName;
        private DateTime m_lastTransactionDate; // this is where we've got to in the list of transactions
        private DateTime m_baseTransactionDate; // this is our starting point in working out what IUDs we have
        // If we don't have baseTransactionDate then we end up sending down inserts and then deletes, and marking
        // inserts as updates (this latter case doesn't matter but it's not quite right)
        private int m_lastId;
        private Guid m_lastGuid;
        private int id;
        public DataTransferParameters()
        {
        }

        public DataTransferParameters(string entityName)
        {
            m_entityName = entityName;
        }

#if MonoDroid
        [a.Runtime.PreserveAttribute] 
#endif
        public string EntityName
        {
            get
            {
                return m_entityName;
            }
            set
            {
                m_entityName = (value);
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = (value);
            }
        }

#if MonoDroid
		[a.Runtime.PreserveAttribute]
#endif
        public DateTime LastTransactionDate
        {
            get
            {
                return m_lastTransactionDate;
            }
            set
            {
                m_lastTransactionDate = (value);
            }
        }

#if MonoDroid
        [a.Runtime.PreserveAttribute] 
#endif
        public DateTime BaseTransactionDate
        {
            get
            {
                return m_baseTransactionDate;
            }
            set
            {
                m_baseTransactionDate = (value);
            }
        }

#if MonoDroid
        [a.Runtime.PreserveAttribute] 
#endif
        public int LastId
        {
            get
            {
                return m_lastId;
            }
            set
            {
                m_lastId = (value);
            }
        }

#if MonoDroid
        [a.Runtime.PreserveAttribute] 
#endif
        public Guid LastGuid
        {
            get
            {
                return m_lastGuid;
            }
            set
            {
                m_lastGuid = (value);
            }
        }

        public override string ToString()
        {
            return m_entityName + "\n" + m_lastTransactionDate.ToString() + "\n" + m_baseTransactionDate.ToString() + "\n" + m_lastId + "\n" + m_lastGuid;
        }
    }
}
