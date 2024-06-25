using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.Database;
using Project.Shared.AuthenticationShared;
using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;
using System.Text.Json;
using System.Text;

namespace Project.Backend.Server.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public OrderController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<List<OrderResponse>> GetOrders()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new List<OrderResponse>();
            }

            var orders = await _databaseContext.OrderTable
                .Where(order => order.FkUserId == userId)
                .ToListAsync();

            var response = new List<OrderResponse>();
            foreach (var order in orders)
            {
                response.Add(new OrderResponse
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    Adress = order.Adress,
                    City = order.City,
                    PostalCode = order.PostalCode,
                    AdditionalInfo = order.AdditionalInfo,
                    FlatNumber = order.FlatNumber,
                    fkOrderStatus = (OrderStatus)order.fkOrderStatus,
                    OrderDate = order.OrderDate,
                });
            }

            return response;
        }

        [HttpGet]
        [Route("returns")]
        public async Task<List<OrderResponse>> GetReturns()
        {
            var orders = await _databaseContext.OrderTable
                .Where(order => order.fkOrderStatus == Database.Tables.OrderStatus.Returning)
                .ToListAsync();

            var response = new List<OrderResponse>();
            foreach (var order in orders)
            {
                response.Add(new OrderResponse
                {
                    Id = order.Id,
                    TotalPrice = order.TotalPrice,
                    Adress = order.Adress,
                    City = order.City,
                    PostalCode = order.PostalCode,
                    AdditionalInfo = order.AdditionalInfo,
                    FlatNumber = order.FlatNumber,
                    fkOrderStatus = (OrderStatus)order.fkOrderStatus,
                    OrderDate = order.OrderDate,
                });
            }

            return response;
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<OrderResponse> GetOrder([FromRoute] string id)
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

        [HttpGet]
        [Route("payment/{id}")]
        public async Task<GetOrderPaymentResponse> GetOrderPayment([FromRoute] string id)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;
            var order = await _databaseContext.OrderTable.FirstOrDefaultAsync(order => order.Id == id);

            if (order is null)
                throw new Exception("Order Not Found");

            // TODO Add Discount Percentage
            var user = await _databaseContext.UserInfoTable.FirstOrDefaultAsync(user => user.Id == userId);
            var points = user!.Points;
            var (discountPointsUse, discountPercentage) = GetDiscount(points);

            return new GetOrderPaymentResponse()
            {
                Id = order.Id,
                Price = order.TotalPrice,
                DiscountPercentage = discountPercentage,
                DiscountPointsUse = discountPointsUse
            };
        }

        [HttpPost]
        [Route("payment/{id}")]
        public async Task DoOrderPayment([FromBody] DoOrderPaymentRequest request, [FromRoute] string id)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;
            var order = await _databaseContext.OrderTable.FirstOrDefaultAsync(order => order.Id == id);

            var price = order!.TotalPrice;
            int? poitsToDetuct = null;
            if (request.UseDiscount)
            {
                var userTable = await _databaseContext.UserInfoTable.FirstOrDefaultAsync(user => user.Id == userId);
                var points = userTable!.Points;

                (poitsToDetuct, var discountPercentage) = GetDiscount(points);
                price *= (100 - discountPercentage) / 100;
            }

            // Payment
            var paymentRequest = new MockPayment()
            {
                Price = price,
                IBAN = request.IBAN,
                SecurityCode = request.SecurityCode,
                Expiration = request.Expiration,
            };
            var stringContent = new StringContent(JsonSerializer.Serialize(paymentRequest), Encoding.UTF8, "application/json");
            var _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };
            var paymentResponse = await _httpClient.PostAsync("/payments", stringContent);
            if (!paymentResponse.IsSuccessStatusCode)
                throw new Exception("Issue with Payment");


            // If used Points deduct
            if (request.UseDiscount)
            {
                var userTable = await _databaseContext.UserInfoTable.FirstOrDefaultAsync(user => user.Id == userId);
                userTable!.Points -= (int)poitsToDetuct!;
                await _databaseContext.SaveChangesAsync();
            }

            // Update order to Paid
            order.fkOrderStatus = Database.Tables.OrderStatus.Waiting;
            _databaseContext.SaveChangesAsync();

            // Add Points
            var user = await _databaseContext.UserInfoTable.FirstOrDefaultAsync(user => user.Id == userId);
            user!.Points += (int)Math.Floor(price);
            await _databaseContext.SaveChangesAsync();
        }

        private (int, double) GetDiscount(int points)
        {
            if (points > 450)
                return (450, 15);

            if (points > 350)
                return (350, 10);

            if (points > 200)
                return (200, 5);

            return (0, 0);
        }

        [HttpPost]
        public async Task<CreateOrderResponse> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;
            var cartItems = await _databaseContext.CartItemTable.Where(item => item.FkUserId == userId && item.FkOrderId == null).ToListAsync();

            var orderId = Guid.NewGuid().ToString();

            // Calculate Price
            double price = 0;
            foreach (var cartItem in cartItems)
            {
                var shopItem = await _databaseContext.ShopItemTable.FirstOrDefaultAsync(shopItem => shopItem.Id == cartItem.FkShopItemId);
                if (shopItem is null)
                    continue;

                price += shopItem.Price * cartItem.Count;
            }

            // Add Order 
            var order = new Database.Tables.OrderTable
            {
                Id = orderId,
                TotalPrice = price,
                Adress = request.Address,
                City = request.City,
                PostalCode = request.PostalCode,
                AdditionalInfo = request.AdditionalInfo,
                FlatNumber = request.FlatNumber,
                fkOrderStatus = (Database.Tables.OrderStatus)request.fkOrderStatus,
                fkOrderType = (Database.Tables.OrderType)request.fkOrderType,
                OrderDate = DateTime.Now,
                FkUserId = userId ?? string.Empty,
            };

            await _databaseContext.OrderTable.AddAsync(order);
            await _databaseContext.SaveChangesAsync();

            // Update CartItems
            foreach (var cartItem in cartItems)
                cartItem.FkOrderId = order.Id;

            await _databaseContext.SaveChangesAsync();

            return new() { OrderId = orderId };
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

        [Route("delete")]
        [HttpPost]
        public async Task DeleteOrder([FromBody] DeleteOrderRequest request)
        {

            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(item => item.Id == request.Id);
            if (item is not null)
                _databaseContext.OrderTable.Remove(item);

            _databaseContext.SaveChanges();
        }

        [Route("status/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetOrderStatus([FromRoute] string id)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(x => x.Id == id);

            if (item?.fkOrderStatus.ToString() != "Cancelled")
            {
                return StatusCode(303, $"/order-cancelation/{id}");
            }

            return Ok(item!.fkOrderStatus.ToString());
        }

        [Route("cancel/{id}")]
        [HttpGet]
        public async Task<OrderResponse> CancelOrder([FromRoute] string id)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(x => x.Id == id);

            item.fkOrderStatus = Database.Tables.OrderStatus.Cancelled;
            await _databaseContext.SaveChangesAsync();

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
        [Route("confirm-return/{id}")]
        [HttpGet]
        public async Task<OrderResponse> ConfirmReturnOrder([FromRoute] string id)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(x => x.Id == id);

            item.fkOrderStatus = Database.Tables.OrderStatus.Waiting;
            await _databaseContext.SaveChangesAsync();

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

        [Route("check-order-date/{id}")]
        [HttpGet]
        public async Task<IActionResult> CheckOrderDateEndpoint([FromRoute] string id)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            if ((DateTime.Now - item.OrderDate).TotalDays <= 3)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Prekės grąžinti nebegalima");
            }
        }

        [Route("submit-item-return/{id}")]
        [HttpPost]
        public async Task<IActionResult> SubmitItemReturn([FromRoute] string id)
        {
            var item = await _databaseContext.OrderTable.FirstOrDefaultAsync(x => x.Id == id);


            item.fkOrderStatus = Database.Tables.OrderStatus.Returning;
            await _databaseContext.SaveChangesAsync();

            return Ok();
        }
    }
}
