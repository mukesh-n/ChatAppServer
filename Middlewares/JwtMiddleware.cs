using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyDotNetAPP.Models;
using MyDotNetAPP.Services;
using MyDotNetAPP.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyDotNetAPP.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        ILogger _logger;
        ApplicationEnvironment _applicationSettings;
        IHttpContextAccessor _httpContextAccessor;
        public JwtMiddleware(
            RequestDelegate next, ILogger<JwtMiddleware> logger,
            IOptions<ApplicationEnvironment> applicationSettings,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _next = next;
            _logger = logger;
            _applicationSettings = applicationSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token == null)
                {
                    token = _httpContextAccessor.HttpContext.Request.Cookies[AppConstants.AccessTokenKey];
                }
                if (token != null)
                    await attachUserToContext(context, token);
                await _next(context);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task attachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_applicationSettings.jwtsecret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero,


                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                UserLoginRes user = new UserLoginRes();
                user.id = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                user.mobilenumber = jwtToken.Claims.First(x => x.Type == "mobilenumber").Value;
                context.Items["User"] = user;
            }
            catch (SecurityTokenExpiredException e)
            {

            }
            catch (Exception)
            {

            }
        }

    }
}
