using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class DeleteCartItemRequest
{
    [Required]
    public string Id { get; set; }

}
