using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Project.Shared.ResponseModels
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Unpaid,
        Waiting,
        BeingDelivered,
        Completed,
        Returning,
        Cancelled
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum OrderType
    {
        TakenItem,
        DeliverItem
    }
    

    public class OrderResponse
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
    }
}
