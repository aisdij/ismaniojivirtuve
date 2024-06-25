using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Backend.Server.Database.Tables
{
    [Table("ProductTag")]
    public class ProductTagTable
    {
        public string FkShopItemId { get; set; }

        public string Name { get; set;}
    }
}