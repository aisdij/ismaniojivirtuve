using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.Database;
using Project.Shared.AuthenticationShared;
using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;

namespace Project.Backend.Server.Controllers
{
    [Route("api/courier-order")]
    [ApiController]
    public class CourierController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public CourierController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderResponse>>> Select()
        {
            return await _databaseContext.OrderTable
                .Select(item => new OrderResponse
                {
                    Id = item.Id,
                    TotalPrice = item.TotalPrice,
                    Adress = item.Adress,
                    City = item.City,
                    PostalCode = item.PostalCode,
                    AdditionalInfo = item.AdditionalInfo,
                    FlatNumber = item.FlatNumber,
                    fkOrderStatus = (OrderStatus)item.fkOrderStatus,
                    fkOrderType = (OrderType)item.fkOrderType,
                    OrderDate = item.OrderDate,
                })
                .ToListAsync();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<OrderResponse> Select([FromRoute] string id)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(x => x.Id == id);

            return new OrderResponse()
            {
                Id = item.Id,
                TotalPrice = item.TotalPrice,
                Adress = item.Adress,
                City = item.City,
                PostalCode = item.PostalCode,
                AdditionalInfo = item.AdditionalInfo,
                FlatNumber = item.FlatNumber,
                fkOrderStatus = (OrderStatus)item.fkOrderStatus,
                fkOrderType = (OrderType)item.fkOrderType,
                OrderDate = item.OrderDate,
            };
        }

        [Route("update")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest request)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(o => o.Id == request.Id);

            if (item == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse<Project.Backend.Server.Database.Tables.OrderStatus>(request.fkOrderStatus.ToString(), true, out var status))
            {
                return BadRequest("Invalid order status.");
            }

            item.fkOrderStatus = status;

            try
            {
                _databaseContext.OrderTable.Update(item);
                await _databaseContext.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
