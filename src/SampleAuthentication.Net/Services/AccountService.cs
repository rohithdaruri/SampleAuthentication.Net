using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleAuthentication.Net.UnitofWork;
using SampleAuthentication.Net.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace SampleAuthentication.Net.Services
{
    public interface IAccountService
    {
        Task<AuthResult> Login(LoginModel model);
        Task<AuthResult> Register(RegisterModel model);
    }
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        public readonly IUnitOfWork _unitOfWork;
        public AccountService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResult> Register(RegisterModel model)
        {
            try
            {
                string UserName = model.Email;
                string Email = model.Email;
                string Password = model.Password;

                var user = new IdentityUser() { UserName = UserName, Email = Email };
                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    //await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, false);

                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                    var roles = await _signInManager.UserManager.GetRolesAsync(appUser);

                    var token = GenerateJwtToken(model.Email, appUser, roles.ToList());
                    return new AuthResult { Errors = null, Successful = true, Token = token, UserId = appUser.Id };
                }
                else
                {
                    var errors = result.Errors.Select(x => x.Description);

                    return new AuthResult { Errors = errors, Successful = false, Token = null, UserId = null };
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public async Task<AuthResult> Login(LoginModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

                if (result.Succeeded)
                {
                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                    var roles = await _signInManager.UserManager.GetRolesAsync(appUser);

                    var token = GenerateJwtToken(model.Email, appUser, roles.ToList());
                    return new AuthResult { Errors = null, Successful = true, Token = token, UserId = appUser.Id };
                }
                else
                {
                    List<string> Errors = new List<string>() { "Invalid Credentials" };
                    return new AuthResult { Errors = Errors, Successful = false, Token = null, UserId = null };
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        private string GenerateJwtToken(string email, IdentityUser user, List<string> roles)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}