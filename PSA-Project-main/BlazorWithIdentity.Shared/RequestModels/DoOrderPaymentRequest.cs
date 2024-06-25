using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Shared.RequestModels;

public class DoOrderPaymentRequest
{
    [Required]
    public string IBAN { get; set; }

    [Required]
    [Range(0, 999)]
    public int SecurityCode { get; set; }

    [Required]
    public string Expiration { get; set;  }

    public bool UseDiscount { get; set; } = false;
}
