using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class DeleteShopItemRequest
{
    [Required]
    public string Id { get; set; }
}
