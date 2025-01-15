using CommunityToolkit.Mvvm.ComponentModel;

namespace BorisMobile.Models
{
    public class WorkOrderList
    {
        public WorkOrders workOrder { get; set; }
        public Customers customer { get; set; }
        public Locations location { get; set; }
        public Audits audit { get; set; }
    }

        public class WorkOrderGroup : ObservableObject
    {
        private bool isExpanded;

        public string MonthName { get; set; }
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }
        //public bool IsExpanded { get; set; } = false; // For collapsible functionality
            public List<WorkOrderList> WorkOrders { get; set; }
    }

    
}
