using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Backend.Server.Database.Tables
{
    [Table("CartItem")]
    public class CartItemTable
    {
        public string Id { get; set; }

        public int Count { get; set; }

        public string FkShopItemId { get; set; }

        public string FkUserId { get; set; }

        public string? FkOrderId { get; set; }
    }
}
