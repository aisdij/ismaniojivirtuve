using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Backend.Server.Database.Tables
{
    public enum OrderStatus
    {
        Unpaid,
        Waiting,
        BeingDelivered,
        Completed,
        Returning,
        Cancelled
    }

    public enum OrderType
    {
        TakenItem,
        DeliveredItem
    }

    [Table("Order")]
    public class OrderTable
    {
        public string Id { get; set; }
        public double TotalPrice { get; set; }
        public string Adress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string AdditionalInfo { get; set; }
        public int FlatNumber { get; set; }
        public OrderStatus fkOrderStatus { get; set; }
        public OrderType fkOrderType { get; set; }
        public DateTime OrderDate { get; set; }
        public string FkUserId { get; set; }
    }
}
