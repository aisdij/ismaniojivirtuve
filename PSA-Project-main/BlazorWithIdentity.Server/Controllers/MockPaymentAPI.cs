using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Project.Backend.Server.Controllers
{
    [Route("payments")]
    [ApiController]
    public class MockPaymentAPI : Controller
    {
        public MockPaymentAPI()
        {
        }

        [HttpPost]
        public async Task<ActionResult> DoPayment()
        {
            return Ok();
        }
    }

    public class MockPayment
    {
        [Required]
        public double Price;
        
        [Required]
        public string IBAN { get; set; }

        [Required]
        [Range(0, 999)]
        public int SecurityCode { get; set; }

        [Required]
        public string Expiration { get; set; }
    }
}
