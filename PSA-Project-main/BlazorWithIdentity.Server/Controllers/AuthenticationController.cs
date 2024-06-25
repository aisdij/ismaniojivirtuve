using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.AuthenticationService;
using Project.Backend.Server.Database;
using Project.Backend.Server.Database.Tables;
using Project.Shared.AuthenticationShared;
using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Project.Backend.Server.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IHashingService _hashingService;
        private readonly ITokenManager _tokenManager;
        private readonly DatabaseContext _databaseContext;

        public AuthenticationController(
            IHashingService hashingService,
            ITokenManager tokenManager,
            DatabaseContext databaseContext)
        {
            _hashingService = hashingService;
            _tokenManager = tokenManager;
            _databaseContext = databaseContext;
        }

        [HttpPost]
        [Route("login")]
        public async Task<LoginResponse> Login([FromBody] LoginRequest request)
        {
            if (request.Email is null || request.Password is null)
                throw new BadHttpRequestException("Bad Request");

            var user = _databaseContext.UserInfoTable
                .FirstOrDefault(u => u.Email == request.Email);

            if (user is null || !_hashingService.Verify(user.PasswordHashed, request.Password))
                throw new BadHttpRequestException("Bad credentials");

            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(CustomJwtClaims.Role, user.Role == null ? string.Empty : user.Role),
                new(CustomJwtClaims.UserId, user.Id),
                new(CustomJwtClaims.Gender, user.Gender == null ? string.Empty : user.Gender)
            };

            var response = new LoginResponse
            {
                JwtToken = _tokenManager.GenerateToken(claims),
            };

            return response;
        }


        [HttpPost]
        [Route("register")]
        public async Task<LoginResponse> Register([FromBody] RegisterRequest request)
        {
            var sameUser = await _databaseContext.UserInfoTable.Where(user => user.Email == request.Email).FirstOrDefaultAsync();
            if (sameUser is not null)
                throw new HttpRequestException("User with provided email already exists", new(), System.Net.HttpStatusCode.Conflict);

            // Table Entry Generation
            var hashedPassword = _hashingService.Hash(request.Password);
            var userInfoTable = new UserInfoTable
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                PasswordHashed = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                Points = 0
            };
            await _databaseContext.AddEntityAsync(userInfoTable);

            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Email, request.Email),
                new(CustomJwtClaims.UserId, userInfoTable.Id),
                new(CustomJwtClaims.Gender, userInfoTable.Gender)
            };

            var response = new LoginResponse
            {
                JwtToken = _tokenManager.GenerateToken(claims),
            };

            return response;
        }
    }
}
