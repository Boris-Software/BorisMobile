using BorisMobile.DataHandler;
using BorisMobile.Models;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using static BorisMobile.XML.ParseXML;

namespace BorisMobile.ViewModels
{
    public partial class ScheduledWorkViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool isVisibelCalenderPicker;

        private DateTime _selectedWordkflowDate;

        private bool isGroupBy { get; set; }
        private bool isDateSelection { get; set; }
        private bool isMinimized { get; set; }
        public DateTime SelectedWorkflowDate
        {
            get => _selectedWordkflowDate;
            set
            {
                if (_selectedWordkflowDate != value)
                {
                    if (value != DateTime.MinValue)
                    {
                        isDateSelection = true;
                        isGroupBy = false;
                        _selectedWordkflowDate = value;
                        OnPropertyChanged();
                        LoadWorkOrders();
                    }
                }
            }
        }
        Item item = null;
        XmlElement oneItem;

        [ObservableProperty]
        private ObservableCollection<WorkOrderGroup> workOrderGroups = new ObservableCollection<WorkOrderGroup>();
        public ObservableCollection<string> GroupOptions { get; set; } = new ObservableCollection<string>();

        private string _selectedGroupBy;
        public string SelectedGroupBy
        {
            get => _selectedGroupBy;
            set
            {
                isGroupBy = true;
                isDateSelection = false;
                if (_selectedGroupBy != value)
                {
                    SelectedWorkflowDate = new DateTime();
                    _selectedGroupBy = value;
                    OnPropertyChanged();
                    LoadWorkOrders();
                }
            }
        }
        List<WorkOrderList> list;
        List<WorkOrderList> DisplayList;
        public ScheduledWorkViewModel(Item item)
        {
            List<string> items = ["Weekly", "Monthly", "Daily"];
            GroupOptions = new ObservableCollection<string>(items);
            _selectedGroupBy = "Monthly";
            isGroupBy = true;
            this.item = item;
            IsVisibelCalenderPicker = false;
            LoadWorkOrders();
        }

        // Group the WorkOrders based on the selected option (Monthly, Weekly, Daily)
        private void LoadWorkOrders()
        {
            if (list == null)
            {
                WorkOrderListHandler workOrderHandler = new WorkOrderListHandler();
                list = workOrderHandler.GetWorkOrderListItem();
                DisplayList = list;
            }
            else
            {
                DisplayList = list;
            }

            if (item.Template.Type.Equals("exclude"))
            {
                DisplayList = DisplayList.Where(workOrder => !item.Template.Entries.Contains(workOrder.workOrder.TemplateId)).ToList();
            }

            if (isDateSelection)
            {
                DisplayList = DisplayList.Where(workOrder => SelectedWorkflowDate.ToString("dd/MM/yyyy").Contains(workOrder.workOrder.WorkOrderDate.ToString("dd/MM/yyyy"))).ToList();
                CalendarIconClicked();
                WorkOrderGroups.Clear();

                if (DisplayList.Count > 0)
                {
                    List<WorkOrderGroup> dateSelectedList = new List<WorkOrderGroup>();
                    var workOrderGroupdata = new WorkOrderGroup
                    {
                        MonthName = DisplayList[0].workOrder.WorkOrderDate.ToString("dd/MM/yyyy"),
                        IsExpanded = true, // Initially collapsed
                        WorkOrders = DisplayList
                    };
                    dateSelectedList.Add(workOrderGroupdata);
                    WorkOrderGroups = new ObservableCollection<WorkOrderGroup>(dateSelectedList);
                }
                return;
            }
            else if (isGroupBy)
            {
                int itemCount = 0;
                IEnumerable<WorkOrderGroup> groupedOrders = null;

                switch (_selectedGroupBy)
                {
                    case "Monthly":
                        groupedOrders = DisplayList
                        .GroupBy(w => w.workOrder.WorkOrderDate.ToString("MMMM yyyy"))
                        .Select(g =>
                        {
                            Console.WriteLine($"Grouping Key: {g.Key}, Count: {g.Count()},{g.ToList().Count},{g.ToList()[0]}");
                            itemCount++;
                            return new WorkOrderGroup
                            {
                                MonthName = g.Key,
                                IsExpanded = itemCount == 1 ? true : false, // Initially collapsed
                                WorkOrders = g.ToList()
                            };
                        }).ToList();
                        break;
                    case "Weekly":
                        //groupedOrders = workOrders.GroupBy(w => $"{CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(w.OrderDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)} - {w.OrderDate.Year}");
                        groupedOrders = DisplayList
                        .GroupBy(w => $"{CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(w.workOrder.WorkOrderDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)} - {w.workOrder.WorkOrderDate.Year}")
                        .Select(g =>
                        {
                            Console.WriteLine($"Grouping Key: {g.Key}, Count: {g.Count()},{g.ToList().Count},{g.ToList()[0]}");
                            itemCount++;
                            return new WorkOrderGroup
                            {
                                MonthName = g.Key,
                                IsExpanded = itemCount == 1 ? true : false, // Initially collapsed
                                WorkOrders = g.ToList()
                            };
                        }).ToList();
                        break;
                    case "Daily":
                        //groupedOrders = workOrders.GroupBy(w => w.OrderDate.Date.ToString("dd MMM yyyy"));
                        groupedOrders = DisplayList
                        .GroupBy(w => w.workOrder.WorkOrderDate.Date.ToString("dd MMM yyyy"))
                        .Select(g =>
                        {
                            Console.WriteLine($"Grouping Key: {g.Key}, Count: {g.Count()},{g.ToList().Count},{g.ToList()[0]}");
                            itemCount++;
                            return new WorkOrderGroup
                            {
                                MonthName = g.Key,
                                IsExpanded = itemCount == 1 ? true : false, // Initially collapsed
                                WorkOrders = g.ToList()
                            };
                        }).ToList();
                        break;
                }

                //set notes from workOrdergroups.workorder.xmldoc
                foreach (var item in groupedOrders)
                {
                    foreach (var subItem in item.WorkOrders)
                    {
                        string notes = GetData(subItem.workOrder.XmlDoc,"notes");
                        subItem.workOrder.Notes = notes;

                        var jobNo = GetData(subItem.workOrder.XmlDoc, "jobno");
                        subItem.workOrder.JobNo = jobNo;
                    }
                }

                WorkOrderGroups.Clear();
                WorkOrderGroups = new ObservableCollection<WorkOrderGroup>(groupedOrders);
            }
        }

        public string GetData(string xmlContent , string type)
        {
            try
            {
                // Load the XML content
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                XmlNode commonNode = null;

                if (type.Equals("notes"))
                {
                    // Navigate to the 'notes' element
                    commonNode = xmlDoc.SelectSingleNode("//WorkOrderItems/notes");
                }
                else if (type.Equals("jobno"))
                {
                    commonNode = xmlDoc.SelectSingleNode("//WorkOrderItems/jobNo");
                }

                string data = null;
                if (commonNode != null && commonNode.Attributes["text"] != null)
                {
                    data = commonNode.Attributes["text"].Value;
                    return !string.IsNullOrEmpty(data) ? data : string.Empty; // Return value or default message
                }

                return data; // Return default message if 'notes' element is missing
            }
            catch (XmlException ex)
            {
                // Handle XML parsing errors gracefully
                Console.WriteLine($"XML parsing error: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                // Handle other exceptions 
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return string.Empty;
            }
        }
        //public async Task Init()
        //{
        //    WorkOrderListHandler workOrderHandler = new WorkOrderListHandler();
        //    List<WorkOrderList> list = workOrderHandler.GetWorkOrderListItem();

        //    if (item.Template.Type.Equals("exclude"))
        //    {
        //        list = list.Where(workOrder => !item.Template.Entries.Contains(workOrder.workOrder.TemplateId)).ToList();
        //    }

        //    //workOrderList = new ObservableCollection<WorkOrderList>(list);

        //    int itemCount = 0;
        //    // Group work orders by Month
        //    var groupedOrders = list
        //        .GroupBy(w => w.workOrder.WorkOrderDate.ToString("MMMM yyyy"))
        //        .Select(g =>
        //        {
        //            Console.WriteLine($"Grouping Key: {g.Key}, Count: {g.Count()},{g.ToList().Count},{g.ToList()[0]}");
        //            itemCount++;
        //            return new WorkOrderGroup
        //            {
        //                MonthName = g.Key,
        //                IsExpanded = itemCount == 1 ? true : false, // Initially collapsed
        //                WorkOrders = g.ToList()
        //            };
        //        }).ToList();

        //    WorkOrderGroups = new ObservableCollection<WorkOrderGroup>(groupedOrders);
        //}

        [RelayCommand]
        public async void ToggleGroup(Models.WorkOrderGroup groupData)
        {
            foreach (var item in WorkOrderGroups)
            {
                if (item.MonthName == groupData.MonthName)
                {
                    item.IsExpanded = !item.IsExpanded;
                    break;
                }
            }
        }

        [RelayCommand]
        public async void GroupBy(string value)
        {
            SelectedGroupBy = value;
        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void Minimize()
        {
            isMinimized = !isMinimized;
            foreach (var item in WorkOrderGroups)
            {
                item.IsExpanded = isMinimized;
            }
        }

        [RelayCommand]
        public async void CalendarIconClicked()
        {
            IsVisibelCalenderPicker = !IsVisibelCalenderPicker;
        }
        

        [RelayCommand]
        public async void JobClick(WorkOrderList item)
        {
            await App.Current.MainPage.Navigation.PushAsync(new JobDetailsPage(new JobDetailsPageViewModel(item)));
        }
    }
}
