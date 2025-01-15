using BorisMobile.Models;
using Microsoft.Data.Sqlite;

namespace BorisMobile.DataHandler
{
    public class WorkOrderListHandler
    {
        public List<WorkOrderList> GetWorkOrderListItem()
        {
            var workOrderList = new List<WorkOrderList>();
            try
            {
                string query = "select w.Id WorkOrderId, w.CustomerId as WorkOrderCustomerID, w.LocationId as WorkOrderLocationId, w.TemplateId, w.WorkOrderDate,w.XmlDoc,w.CustomOrder,w.CustomData,w.SupplementaryXmlDocument," +
                    "w.IsInitialised,w.GeneratingXmlDocument,w.OriginalCustomOrder,w.GroupId,w.AppStatusId,w.UserId,c.Id as customerID, c.Desc as CustomerDesc, c.XmlDoc as CustomerXmlDoc, l.Id as LocationID, " +
                    "l.Desc as LocationDesc, l.CustomerId as LocationCustomerID, l.FilterListEntryId, l.XmlDoc as LocationXmlDoc,a.Id as AuditsId, a.Desc as AuditsDesc, a.XmlDoc as AuditsXmlDoc, a.Credits as AuditsCredit " +
                    "from WorkOrders w inner join Customers c on w.CustomerId=c.Id inner join Locations l on w.LocationId = l.Id inner join Audits a on w.TemplateId = a.Id where w.UserId = " + Convert.ToInt32(Preferences.Get("UserId", 0)) + " and w.TemplateId is not 0";



                //using (var connection = new SqliteConnection("Data Source=YourDatabaseFilePath"))
                //{
                //connection.Open();

                using (var command = new SqliteCommand(query, App.DatabaseConnection))
                {
                    // Add parameters to avoid SQL injection
                    //command.Parameters.AddWithValue("@userId", Convert.ToInt32(Preferences.Get("UserId", 0));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long ticks = reader.GetInt64(reader.GetOrdinal("WorkOrderDate"));
                            DateTime utcDateTime = new DateTime(ticks, DateTimeKind.Utc);

                            int customOrderOrdinal = reader.GetOrdinal("CustomOrder");
                            int customerDataOrdinal = reader.GetOrdinal("CustomData");
                            int supplementrayxmlDocumentOrdinal = reader.GetOrdinal("SupplementaryXmlDocument");
                            int generatingXmlDocumentOrdinal = reader.GetOrdinal("GeneratingXmlDocument");
                            int originalCustomerOrderOrdinal = reader.GetOrdinal("OriginalCustomOrder");

                            // Map the data to the WorkOrderList object
                            var workOrderListItem = new WorkOrderList
                            {
                                workOrder = new WorkOrders
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("WorkOrderId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("WorkOrderCustomerID")),
                                    LocationId = reader.GetInt32(reader.GetOrdinal("WorkOrderLocationId")),
                                    TemplateId = reader.GetInt32(reader.GetOrdinal("TemplateId")),
                                     WorkOrderDate = utcDateTime,
                                    XmlDoc = reader.GetString(reader.GetOrdinal("XmlDoc")),
                                    CustomOrder = !reader.IsDBNull(customOrderOrdinal) ? reader.GetString(reader.GetOrdinal("CustomOrder")) : null,
                                    CustomData = !reader.IsDBNull(customerDataOrdinal) ? reader.GetString(reader.GetOrdinal("CustomData")) : null,
                                    SupplementaryXmlDocument = !reader.IsDBNull(supplementrayxmlDocumentOrdinal) ?  reader.GetString(reader.GetOrdinal("SupplementaryXmlDocument")) : null,
                                    IsInitialised = reader.GetInt32(reader.GetOrdinal("IsInitialised")),
                                    GeneratingXmlDocument = !reader.IsDBNull(generatingXmlDocumentOrdinal) ?  reader.GetString(reader.GetOrdinal("GeneratingXmlDocument")):null,
                                    OriginalCustomOrder = !reader.IsDBNull(originalCustomerOrderOrdinal) ?  reader.GetString(reader.GetOrdinal("OriginalCustomOrder")) : null,
                                    GroupId = reader.GetInt32(reader.GetOrdinal("GroupId")),
                                    AppStatusId = reader.GetInt32(reader.GetOrdinal("AppStatusId")),
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                },
                                customer = new Customers
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("customerID")),
                                    Desc = reader.GetString(reader.GetOrdinal("CustomerDesc")),
                                    XmlDoc = reader.GetString(reader.GetOrdinal("CustomerXmlDoc"))
                                },
                                location = new Locations
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("LocationID")),
                                    Desc = reader.GetString(reader.GetOrdinal("LocationDesc")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("LocationCustomerID")),
                                    FilterListEntryId = reader.GetInt32(reader.GetOrdinal("FilterListEntryId")),
                                    XmlDoc = reader.GetString(reader.GetOrdinal("LocationXmlDoc"))
                                },
                                audit = new Audits
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("AuditsId")),
                                    Desc = reader.GetString(reader.GetOrdinal("AuditsDesc")),
                                    Credits = reader.GetString(reader.GetOrdinal("AuditsCredit")),
                                    XmlDoc = reader.GetString(reader.GetOrdinal("AuditsXmlDoc"))
                                }
                            };

                            workOrderList.Add(workOrderListItem);
                        }
                    }
                }
                
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return workOrderList;
        }
    }
}
