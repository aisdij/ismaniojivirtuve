using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Shared.RequestModels;

public class UpdateCartItemRequest
{
	[Required]
    public string Id { get; set; }

	[Required]
	[Range(0, int.MaxValue)]
	public int Count { get; set; }
}
