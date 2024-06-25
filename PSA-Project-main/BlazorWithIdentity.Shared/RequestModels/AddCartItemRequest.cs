using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class AddCartItemRequest
{
    [Required]
    public string FkShopItemId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Count { get; set; }

}
