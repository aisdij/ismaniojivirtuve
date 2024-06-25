using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.Database;
using Project.Shared.AuthenticationShared;
using Project.Shared.ResponseModels;

namespace Project.Backend.Server.Controllers
{
    [Route("api/lucky-wheel")]
    [ApiController]
    public class LuckyWheelController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public LuckyWheelController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public async Task<LuckyWheelResponse> SpinLuckyWheel()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == CustomJwtClaims.UserId)?.Value;

            var user = await _databaseContext.UserInfoTable.FirstOrDefaultAsync(user => user.Id == userId);

            if (user.LastTimeWheelSpin != null && user.LastTimeWheelSpin.Value.AddHours(24) > DateTime.Now)
            {
                return new LuckyWheelResponse
                {
                    LuckyNumber = 0,
                    LastTimeWheelSpin = (DateTime) user.LastTimeWheelSpin
                };
            }

            int luckyNumber = GetLuckyNumber();
            int points = GetPoints(luckyNumber);
            
            user.LastTimeWheelSpin = DateTime.Now;
            user.Points += points;

            await _databaseContext.SaveChangesAsync();

            return new LuckyWheelResponse
            {
                LuckyNumber = luckyNumber,
                LastTimeWheelSpin = (DateTime) user.LastTimeWheelSpin
            };
        }

        private int GetLuckyNumber()
        {
            var random = new Random();
            return random.Next(1, 6);
        }

        private int GetPoints(int luckyNumber)
        {
            switch (luckyNumber)
            {
                case 1:
                    return 5;
                case 2:
                    return 10;
                case 3:
                    return 20;
                case 4:
                    return 1;
                case 5:
                    return 40;
                default:
                    return 0;
            }
        }
    }
}
