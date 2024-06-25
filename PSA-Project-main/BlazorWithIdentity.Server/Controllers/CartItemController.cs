using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.Database;
using Project.Shared.AuthenticationShared;
using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;

namespace Project.Backend.Server.Controllers
{
    [Route("api/cartItems")]
    [ApiController]
    public class CartItemController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public CartItemController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<List<CartItemResponse>> GetCartItems()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;

            var cartItems = await _databaseContext.CartItemTable.Where(cartItem => cartItem.FkUserId == userId && cartItem.FkOrderId == null).ToListAsync();
            var response = new List<CartItemResponse>();
            foreach (var cartItem in cartItems)
            {
                var shopItem = await _databaseContext.ShopItemTable.Where(shopItem => shopItem.Id == cartItem.FkShopItemId).FirstOrDefaultAsync();
                if (shopItem is null)
                    continue;

                response.Add(new()
                {
                    Id = cartItem.Id,
                    Count = cartItem.Count,
                    ShopItemName = shopItem.Name
                });
            }

            return response;
        }

        [Route("add")]
        [HttpPost]
        public async Task AddShopItemToCart([FromBody] AddCartItemRequest request)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;

            await _databaseContext.CartItemTable.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(), 
                Count = request.Count,
                FkUserId = userId,
                FkShopItemId = request.FkShopItemId,
            });

            await _databaseContext.SaveChangesAsync();
        }

        [Route("update")]
        [HttpPost]
        public async Task UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            var item = await _databaseContext.CartItemTable.FirstOrDefaultAsync(item => item.Id == request.Id);
            if (item is not null)
                item.Count = request.Count;

            await _databaseContext.SaveChangesAsync();
        }

        [Route("delete")]
        [HttpPost]
        public async Task RemoveCartItem([FromBody] DeleteCartItemRequest request)
        {
            var item = await _databaseContext.CartItemTable.FirstOrDefaultAsync(item => item.Id == request.Id);
            if (item is not null)
                _databaseContext.CartItemTable.Remove(item);

            await _databaseContext.SaveChangesAsync();
        }
    }
}
