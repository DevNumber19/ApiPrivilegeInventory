using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using apiDealManagement.Model.Response;
using apiDealManagement.Models.Requests;
using apiDealManagement.Models;

namespace apiDealManagement.Service
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        AuthenticateResponse Logout(string token);
        DealUser GetByToken(string token);
    }
    public class UserService : IUserService
    {

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications

        private readonly JwtConfig _jwtConfig;
        private readonly ConnectionStringConfig _connectionString;

        public UserService(IOptions<JwtConfig> jwtConfig, IOptions<ConnectionStringConfig> connectionString)
        {
            _jwtConfig = jwtConfig.Value;
            _connectionString = connectionString.Value;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            try
            {
                using (var context = new DealManagementDbContext(_connectionString.DefaultConnection))
                {
                    var user = context.UserProfiles.SingleOrDefault(x => x.email == model.Email && x.enabled == 1);

                    // return null if user not found
                    if (user == null) return null;

                    var user_deal = context.DealUsers.SingleOrDefault(x => x.user_id == user.id && x.status == 1);

                    // return null if user not found
                    if (user_deal == null) return null;

                    // authentication successful so generate jwt token
                    var token = generateJwtToken(user);

                    user_deal.token = token;
                    user_deal.updated_at = DateTime.Now;
                    user_deal.updated_by = "API LOGIN";
                    context.SaveChanges();

                    string name = user.first_name + " " + user.last_name;

                    return new AuthenticateResponse(token, true, "successful", user.email, user.id, user_deal.is_admin, name);
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        public AuthenticateResponse Logout(string token)
        {
            using (var context = new DealManagementDbContext(_connectionString.DefaultConnection))
            {
                //var user = context.Users.FirstOrDefault(x => x.token == token);
                var user = context.DealUsers.FirstOrDefault(x => x.token == token);
                if (user != null)
                {
                    user.token = "";
                    context.SaveChanges();
                    return new AuthenticateResponse("", true, "successful", null, null, null, null);
                }
                return new AuthenticateResponse("", false, "unsuccessful", null, null, null, null);

            }
        }

        public DealUser GetByToken(string token)
        {
            using (var context = new DealManagementDbContext(_connectionString.DefaultConnection))
            {
                return context.DealUsers.FirstOrDefault(x => x.token == token);
            }
        }

        // helper methods
        private string generateJwtToken(UserProfile user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.id.ToString()),
                    new Claim(ClaimTypes.Email, user.email.ToString()),
                    //new Claim(ClaimTypes.Name, user.first_name.ToString() + " " + user.last_name.ToString())
                }),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
