using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using ToDoList.Dtos;
using ToDoList.Helpers;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly IMapper _mapper;

        private readonly JwtOption _jwtOption;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(UserManager<AppUser> userManager, IMapper mapper, JwtOption jwtOption, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _mapper = mapper;
            _jwtOption = jwtOption;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<object> LoginAsync(LoginDto LoginRequest)
        {
            if (LoginRequest is null)
                return new AuthDto { Message = "There is an error in data" };

            var user = await _userManager.FindByNameAsync(LoginRequest.UserName);
            if (user is null)
                return new AuthDto { Message = "Username or Password is not correct !" };

            var passcheck = await _userManager.CheckPasswordAsync(user, LoginRequest.Password);
            if (!passcheck)
                return new AuthDto { Message = "Username or Password is not correct !" };

            if (user.BlockUntil.HasValue && user.BlockUntil > DateTime.Now)
                return new AuthDto { Message = "User is blocked until " + user.BlockUntil };

            var token = await _createToken(user);

            var roles = await _userManager.GetRolesAsync(user);

            var version = _httpContextAccessor.HttpContext?.GetRequestedApiVersion()?.ToString();

            if (version == "2")
            {
                return new LoginResponseDto
                {
                    Message = "User is logged in",
                    IsAuthenticated = true,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    ExpiresOn = token.ValidTo
                };
            }

            return new AuthDto
            {
                Message = "User is logged in",
                IsAuthenticated = true,
                UserName = user.UserName,
                Email = user.Email,
                ExpiresOn = token.ValidTo,
                Roles = roles.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        //public async Task<AuthDto> LoginV1Async(LoginDto LoginRequest)

        //{
        //    if (LoginRequest is null)
        //        return new AuthDto { Message = "There is an error in data" };

        //    var user = await _userManager.FindByNameAsync(LoginRequest.UserName);
        //    if (user is null)
        //        return new AuthDto { Message = "Username or Password is not correct !" };

        //    var passcheck = await _userManager.CheckPasswordAsync(user, LoginRequest.Password);
        //    if (!passcheck)
        //        return new AuthDto { Message = "Username or Password is not correct !" };

        //    if (user.BlockUntil.HasValue && user.BlockUntil > DateTime.Now)
        //        return new AuthDto { Message = "User is blocked until " + user.BlockUntil };

        //    var token = await _createToken(user);

        //    var roles = await _userManager.GetRolesAsync(user);

        //    return new AuthDto
        //    {
        //        Message = "User is logged in",
        //        IsAuthenticated = true,
        //        UserName = user.UserName,
        //        Email = user.Email,
        //        ExpiresOn = token.ValidTo,
        //        Roles = roles.ToList(),
        //        Token = new JwtSecurityTokenHandler().WriteToken(token)
        //    };
        //}

        //public async Task<LoginResponseDto> LoginV2Async(LoginDto LoginRequest)
        //{
        //    if (LoginRequest is null)
        //        return new LoginResponseDto { Message = "There is an error in data" };

        //    var user = await _userManager.FindByNameAsync(LoginRequest.UserName);
        //    if (user is null)
        //        return new LoginResponseDto { Message = "Username or Password is not correct !" };

        //    var passcheck = await _userManager.CheckPasswordAsync(user, LoginRequest.Password);
        //    if (!passcheck)
        //        return new LoginResponseDto { Message = "Username or Password is not correct !" };

        //    if (user.BlockUntil.HasValue && user.BlockUntil > DateTime.Now)
        //        return new LoginResponseDto { Message = "User is blocked until " + user.BlockUntil };

        //    var token = await _createToken(user);

        //    var roles = await _userManager.GetRolesAsync(user);

        //    return new LoginResponseDto
        //    {
        //        Message = "User is logged in",
        //        IsAuthenticated = true,
        //        UserName = user.UserName,
        //        Email = user.Email,
        //        Roles = roles.ToList(),
        //        Token = new JwtSecurityTokenHandler().WriteToken(token),
        //        ExpiresOn = token.ValidTo
        //    };
        //}

        public async Task<AuthDto> RegisterV1Async(RegisterDto RegisterRequest)
        {
            if(RegisterRequest is null)
                return new AuthDto {Message = "There is an error in data" };

            if (await _userManager.FindByEmailAsync(RegisterRequest.Email) is not null && await _userManager.FindByNameAsync(RegisterRequest.UserName) is not null)
                return new AuthDto { Message = "Email or Username is registered before !" };

            AppUser user = _mapper.Map<AppUser>(RegisterRequest);
            var result = await _userManager.CreateAsync(user, RegisterRequest.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var item in result.Errors)
                {
                    errors += $"{item.Description} , ";
                }
                return new AuthDto { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            return new AuthDto { Message = "User is created", IsRegister = true };
        }

        public async Task<RegisterResponseDto> RegisterV2Async(RegisterDto RegisterRequest)
        {
            if (RegisterRequest is null)
                return new RegisterResponseDto { Message = "There is an error in data" };

            if (await _userManager.FindByEmailAsync(RegisterRequest.Email) is not null && await _userManager.FindByNameAsync(RegisterRequest.UserName) is not null)
                return new RegisterResponseDto { Message = "Email or Username is registered before !" };

            AppUser user = _mapper.Map<AppUser>(RegisterRequest);
            var result = await _userManager.CreateAsync(user, RegisterRequest.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var item in result.Errors)
                {
                    errors += $"{item.Description} , ";
                }
                return new RegisterResponseDto { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            return new RegisterResponseDto { Message = "User is created", IsRegister = true };
        }

        public async Task<UserDto> UpdateAsync(string userId , UpdateUserDto updateRequest)
        {
            if (updateRequest is null || userId is null)
                throw new ArgumentNullException("There is an error in data" );

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new Exception("There is no user with this id");

            user.FirstName = updateRequest.FirstName;
            user.LastName = updateRequest.LastName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var item in result.Errors)
                {
                    errors += $"{item.Description} , ";
                }
                throw new Exception(errors);
            }

            var userUpdate = _mapper.Map<UserDto>(user);

            return userUpdate;
        }

        private async Task<JwtSecurityToken> _createToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim(ClaimTypes.Role, role));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symetricsecuritykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.Signingkey));
            var signingcred = new SigningCredentials(symetricsecuritykey, SecurityAlgorithms.HmacSha256);

            var Token = new JwtSecurityToken
                (
                issuer: _jwtOption.Issuer,
                audience: _jwtOption.Audience,
                expires: DateTime.Now.AddDays(_jwtOption.Lifetime),
                claims: claims,
                signingCredentials: signingcred
                );

            return Token;
        }
    }
}
