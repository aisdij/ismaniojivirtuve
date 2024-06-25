using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class CreateShopItemRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public int Count { get; set; }

    public string Description { get; set; }
}
