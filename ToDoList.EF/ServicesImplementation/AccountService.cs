using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ToDoList.Core.Models;
using ToDoList.Core.Dtos;
using ToDoList.Services;
using Microsoft.AspNetCore.Http;

namespace ToDoList.EF.ServicesImplementation
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
                var loginResponseDto = new LoginResponseDto();
                if(user.RefreshTokens.Any(t => t.IsActive))
                {
                    var ActiveRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);

                    loginResponseDto.RefreshToken = ActiveRefreshToken.Token;
                    loginResponseDto.RefreshTokenExpiration = ActiveRefreshToken.ExpireOn;
                }
                else
                {
                    RefreshToken refreshToken = _generateRefreshToken();

                    loginResponseDto.RefreshToken = refreshToken.Token;
                    loginResponseDto.RefreshTokenExpiration = refreshToken.ExpireOn;

                    user.RefreshTokens.Add(refreshToken);
                    await _userManager.UpdateAsync(user);
                }
                loginResponseDto.Message = "User is logged in";
                loginResponseDto.IsAuthenticated = true;
                loginResponseDto.UserName = user.UserName;
                loginResponseDto.Email = user.Email;
                loginResponseDto.Roles = roles.ToList();
                loginResponseDto.Token = new JwtSecurityTokenHandler().WriteToken(token);

                return loginResponseDto;
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
                expires: DateTime.Now.AddMinutes(_jwtOption.Lifetime),
                claims: claims,
                signingCredentials: signingcred
                );

            return Token;
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string token)
        {
            var loginResponse = new LoginResponseDto();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if(user is null)
            {
                loginResponse.Message = "Invalid Token";
                return loginResponse;
            }

            var refreshToken = user.RefreshTokens.First(t => t.Token == token);

            if (!refreshToken.IsActive)
            {
                loginResponse.Message = "Inactive Token";
                return loginResponse;
            }

            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = _generateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwtToken = await _createToken(user);

            loginResponse.IsAuthenticated = true;
            loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            loginResponse.Email = user.Email;
            loginResponse.UserName = user.UserName;
            var roles = await _userManager.GetRolesAsync(user);
            loginResponse.Roles = roles.ToList();
            loginResponse.RefreshToken = newRefreshToken.Token;
            loginResponse.RefreshTokenExpiration = newRefreshToken.ExpireOn;

            return loginResponse;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user is null)
                return false;

            var refreshToken = user.RefreshTokens.First(t => t.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            
            return true;
        }

        private RefreshToken _generateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var generator = new RNGCryptoServiceProvider();

            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                CreateOn = DateTime.UtcNow,
                Token = Convert.ToBase64String(randomNumber),
                ExpireOn = DateTime.UtcNow.AddDays(10),
            };
        }

    }
}
