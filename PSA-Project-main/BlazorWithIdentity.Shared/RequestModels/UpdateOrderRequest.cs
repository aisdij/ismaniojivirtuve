using Project.Shared.ResponseModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class UpdateOrderRequest
{
    [Required]
    public string Id { get; set; }
    [Required]
    public OrderStatus fkOrderStatus { get; set; }
}
