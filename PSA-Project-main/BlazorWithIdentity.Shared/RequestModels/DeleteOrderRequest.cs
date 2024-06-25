using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class DeleteOrderRequest
{
    [Required]
    public string Id { get; set; }
}
