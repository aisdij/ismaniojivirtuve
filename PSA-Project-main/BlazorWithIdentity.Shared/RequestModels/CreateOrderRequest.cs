using Project.Shared.ResponseModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class CreateOrderRequest
{
    [Required]
    public string Address { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string PostalCode { get; set; }

    public string AdditionalInfo { get; set; } = string.Empty;

    [Required]
    public int FlatNumber { get; set; }

    [Required]
    public OrderStatus fkOrderStatus { get; set; }

    [Required]
    public OrderType fkOrderType { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }
}
