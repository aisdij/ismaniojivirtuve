using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Backend.Server.Database.Tables
{
    [Table("ShopItem")]
    public class ShopItemTable
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public int Count { get; set; }

        public string Description { get; set; }
    }
}
