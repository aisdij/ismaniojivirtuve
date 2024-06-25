using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.Database;
using Project.Backend.Server.Database.Tables;
using Project.Shared.AuthenticationShared;
using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;

namespace Project.Backend.Server.Controllers
{
    [Route("api/shopItems")]
    [ApiController]
    public class ShopItemsController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public ShopItemsController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<List<ShopItemResponse>> GetShopItems()
        {
            return await _databaseContext.ShopItemTable
                .Select(item => new ShopItemResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Count = item.Count,
                    Description = item.Description,
                })
                .ToListAsync();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ShopItemResponse> GetShopItem([FromRoute] string id)
        {
            var item = await _databaseContext.ShopItemTable.FirstOrDefaultAsync(x => x.Id == id);

            return new ShopItemResponse()
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Count = item.Count,
                Description = item.Description,
            };
        }

        [HttpPost]
        public async Task CreateShopItem([FromBody] CreateShopItemRequest request)
        {
            await _databaseContext.ShopItemTable.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Price = request.Price,
                Count = request.Count,
                Description = request.Description,
            });

            await _databaseContext.SaveChangesAsync();
        }

        [Route("update")]
        [HttpPost]
        public async Task UpdateShopItem([FromBody] UpdateShopItemRequest request)
        {
            var item = await _databaseContext.ShopItemTable.FirstOrDefaultAsync(item => item.Id == request.Id);

            item.Id = request.Id;
            item.Name = request.Name;
            item.Price = request.Price;
            item.Count = request.Count;
            item.Description = request.Description;

            _databaseContext.ShopItemTable.Update(item);
            _databaseContext.SaveChanges();
        }

        [Route("delete")]
        [HttpPost]
        public async Task DeleteShopITem([FromBody] DeleteShopItemRequest request)
        {

            var item = await _databaseContext.ShopItemTable.FirstOrDefaultAsync(item => item.Id == request.Id);
            if (item is not null)
                _databaseContext.ShopItemTable.Remove(item);

            _databaseContext.SaveChanges();
        }

        [Route("recommendations")]
        [HttpGet]
        /// <summary>
        /// Retrieves a list of recommended shop items based on the user's previous orders and current cart items.
        /// </summary>
        /// <returns>A list of recommended shop items.</returns>
        public async Task<List<ShopItemResponse>> GetRecomendations()
        {
            var userID = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;

            var recommendations = new List<ShopItemResponse>();

            var totalOrderedItems = await GetTotalOrderedItems();            

            if (totalOrderedItems.Count >= 3) 
            {
                // Get user's current cart items
                // This is done by getting all items where FkUserId == userID and FkOrderId == null
                var cartItems = await _databaseContext.CartItemTable
                    .Where(test => test.FkUserId == userID && test.FkOrderId == null)
                    .ToListAsync();

                if (cartItems.Count > 0)
                {
                    // Get the product tags for the first item in the cart
                    var firstCartItemTags = await _databaseContext.ProductTagTable
                        .Where(tag => tag.FkShopItemId == cartItems[0].Id)
                        .ToListAsync();

                    // Check if all items in the cart have at least one common tag
                    bool allShareCommonTag = true;
                    foreach (var item in cartItems)
                    {
                        var itemTags = await _databaseContext.ProductTagTable
                            .Where(tag => tag.FkShopItemId == item.Id)
                            .Select(tag => tag.Name)
                            .ToListAsync();

                        if (!itemTags.Any(tag => firstCartItemTags.Select(ct => ct.Name).Contains(tag)))
                        {
                            allShareCommonTag = false;
                            break;
                        }
                    }

                    if (allShareCommonTag)
                    {
                        var commonTagName = firstCartItemTags.First().Name;
                        var topItems = await GetTopMostBoughtItemsExceptTag(3, exceptTag: commonTagName);
                    }
                    else
                    {
                        var commonTagName = firstCartItemTags.First().Name;
                        var topItems = await GetTopMostBoughtItemsWithTag(3, withTag: commonTagName);
                    }
                }
                else
                {
                    var topItems = await GetTopMostBoughtItems(totalOrderedItems, 3);
                    recommendations.AddRange(topItems);
                }
            } else {
                // If there are less than 3 ordered items, het recommendations based on the user's gender
                var user = await _databaseContext.UserInfoTable.FirstOrDefaultAsync(test => test.Id == userID);

                if (user.Gender == "male") 
                {
                    var orderItems = await GetOrderedItemsByGender("male");
                    
                    var topItems = await GetTopMostBoughtItems(orderItems, 3);
                    recommendations.AddRange(topItems);
                }

                if (user.Gender == "female") 
                {
                    var orderItems = await GetOrderedItemsByGender("female");

                    var topItems = await GetTopMostBoughtItems(orderItems, 3);
                    recommendations.AddRange(topItems);
                }

                if (user.Gender != "male" && user.Gender != "female")
                {
                    var topItems = await GetTopMostBoughtItems(totalOrderedItems, 3);
                    recommendations.AddRange(topItems);
                }
            }

            // If there are less than 3 recommendations, add random items to the list
            if (recommendations.Count >= 3)
            {
                return recommendations;
            }
            else
            {
                var randomItems = SelectRandomItems(3 - recommendations.Count);
                recommendations.AddRange(randomItems);
            }

            return recommendations;
        }

        /// <summary>
        /// Get all the items ordered by the current user.
        /// </summary>
        /// <returns>A list of items ordered by the current user</returns>
        private async Task<List<ShopItemResponse>> GetTotalOrderedItems()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;

            var orders = await _databaseContext.OrderTable.Where(test => test.FkUserId == userId).ToListAsync();

            var items = new List<ShopItemResponse>();

            foreach (var order in orders)
            {
                var orderItems = await _databaseContext.CartItemTable.Where(test => test.FkOrderId == order.Id).ToListAsync();

                foreach (var orderItem in orderItems)
                {
                    var item = await _databaseContext.ShopItemTable.FirstOrDefaultAsync(test => test.Id == orderItem.FkShopItemId);

                    items.Add(new ShopItemResponse()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        Count = item.Count,
                        Description = item.Description,
                    });
                }
            }

            return items;
        }

        /// <summary>
        /// /// Retrieves a list of ordered shop items based on the user's gender.
        /// </summary>
        /// <param name="userGender">The gender of the user.</param>
        /// <returns>A list of <see cref="ShopItemResponse"/>.</returns>
        private async Task<List<ShopItemResponse>> GetOrderedItemsByGender(string userGender)
        {
            var users = await _databaseContext.UserInfoTable.Where(user => user.Gender == userGender).ToListAsync();
            var items = new List<ShopItemResponse>();

            foreach (var user in users)
            {
                var orders = await _databaseContext.OrderTable.Where(order => order.FkUserId == user.Id).ToListAsync();

                foreach (var order in orders)
                {
                    var orderItems = await _databaseContext.CartItemTable.Where(item => item.FkOrderId == order.Id).ToListAsync();

                    foreach (var orderItem in orderItems)
                    {
                        var item = await _databaseContext.ShopItemTable.FirstOrDefaultAsync(test => test.Id == orderItem.FkShopItemId);

                        var shopItem = new ShopItemResponse()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Price = item.Price,
                            Count = item.Count,
                            Description = item.Description,
                        };

                        items.Add(shopItem);
                    }
                }
            }

            return items;
        }

        /// <summary>
        ///  Retrieves the top most bought items from a list of shop items.
        /// </summary>
        /// <param name="shopItems">A list of shop items</param>
        /// <param name="count">The number of top items to return</param>
        /// <returns>A list of the top most bought items</returns>
        private async Task<List<ShopItemResponse>> GetTopMostBoughtItems(List<ShopItemResponse> shopItems, int count)
        {
            var items = shopItems;

            return await Task.FromResult(items.OrderByDescending(item => item.Count).Take(count).ToList());
        }

        /// <summary>
        /// Get the top most bought items, excluding items with the specified tag.
        /// </summary>
        /// <param name="amount">The number of top items to return</param>
        /// <param name="exceptTag">The tag to exclude from the search</param>
        /// <returns>A list of the top most bought items, excluding items with the specified tag</returns>
        private async Task<List<ShopItemResponse>> GetTopMostBoughtItemsExceptTag(int amount, string exceptTag)
        {
            // Get all items that don't have the specified tag
            var itemsWithoutTag = await _databaseContext.ProductTagTable
                .Where(tag => tag.Name != exceptTag)
                .Select(tag => tag.FkShopItemId)
                .ToListAsync();

            // Get the top most bought items, excluding items with the specified tag
            var topItems = await _databaseContext.CartItemTable
                .Where(item => itemsWithoutTag.Contains(item.FkShopItemId))
                .GroupBy(item => item.FkShopItemId)
                .OrderByDescending(group => group.Count())
                .Take(amount)
                .Select(group => group.Key)
                .ToListAsync();

            // Convert the item IDs to ShopItemResponse objects
            var topItemResponses = new List<ShopItemResponse>();
            foreach (var itemId in topItems)
            {
                var item = await _databaseContext.ShopItemTable.FindAsync(itemId);
                topItemResponses.Add(new ShopItemResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Count = item.Count,
                    Description = item.Description,
                });
            }

            return topItemResponses;
        }

        /// <summary>
        /// Get the top most bought items with the specified tag.
        /// </summary>
        /// <param name="amount">The number of top items to return</param>
        /// <param name="withTag">The tag to include in the search</param>
        /// <returns>A list of the top most bought items, including only items with the specified tag</returns>
        public async Task<List<ShopItemResponse>> GetTopMostBoughtItemsWithTag(int amount, string withTag)
        {
            // Get all items that have the specified tag
            var itemsWithTag = await _databaseContext.ProductTagTable
                .Where(tag => tag.Name == withTag)
                .Select(tag => tag.FkShopItemId)
                .ToListAsync();

            // Get the top most bought items, including only items with the specified tag
            var topItems = await _databaseContext.CartItemTable
                .Where(item => itemsWithTag.Contains(item.FkShopItemId))
                .GroupBy(item => item.FkShopItemId)
                .OrderByDescending(group => group.Count())
                .Take(amount)
                .Select(group => group.Key)
                .ToListAsync();

            // Convert the item IDs to ShopItemResponse objects
            var topItemResponses = new List<ShopItemResponse>();
            foreach (var itemId in topItems)
            {
                var item = await _databaseContext.ShopItemTable.FindAsync(itemId);
                topItemResponses.Add(new ShopItemResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Count = item.Count,
                    Description = item.Description,
                });
            }

            return topItemResponses;
        }

        /// <summary>
        /// Selects a random number of items from the database.
        /// </summary>
        /// <param name="count">The number of random items to select</param>
        /// <returns>A list of randomly selected items</returns>
        public List<ShopItemResponse> SelectRandomItems(int count)
        {
            var allItems = _databaseContext.ShopItemTable.ToList();
            var random = new Random();
            return allItems.OrderBy(item => random.Next()).Take(count).Select(item => new ShopItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Count = item.Count,
                Description = item.Description,
            }).ToList();
        }
    }
}
