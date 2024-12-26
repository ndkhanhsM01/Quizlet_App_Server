using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Quizlet_App_Server
{
    public class JwtService
    {
        protected readonly UserService userService;
        private readonly IConfiguration config;

        public JwtService(UserService userService, IConfiguration config) 
        { 
            this.userService = userService;
            this.config = config;
        }

        public async Task<Dictionary<string, object>> Authenticate(UserLoginRequest loginReq)
        {
            if(string.IsNullOrWhiteSpace(loginReq.LoginName) || string.IsNullOrWhiteSpace(loginReq.LoginPassword))
            {
                return null;
            }

            var existingUser = userService.FindByLoginName(loginReq.LoginName);
            if (existingUser == null)
            {
                return null;
            }

            if (userService.CheckSuspendTemp(existingUser))
            {
                return null;
            }
            else if(existingUser.TryLoginCount <= 0)
            {
                userService.ResetLoginCount(ref existingUser);
            }

            // password incorrect
            bool isCorrectPassword = userService.VerifyPassword(existingUser.Id, loginReq.LoginPassword);
            if (!isCorrectPassword)
            {
                return null;
            }

            Dictionary<string, object> result = new();

            // jwt token
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            var key = config["Jwt:Key"];
            var tokenValidityMins = config.GetValue<int>("Jwt:TokenValidityMins");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Name, loginReq.LoginName)
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    SecurityAlgorithms.HmacSha512Signature),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            result.Add("accessToken", accessToken);
            result.Add("user", existingUser);

            return result;
        }
    }
}
