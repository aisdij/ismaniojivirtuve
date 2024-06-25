using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Shared.ResponseModels;

public class ShopItemResponse
{
    public string Id { get; set; }

    public string Name { get; set; }

    public double Price { get; set; }

    public int Count { get; set; }

    public string Description { get; set; }
}
