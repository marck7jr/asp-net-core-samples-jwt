using AspNetCoreSamplesJwt.Contracts;
using AspNetCoreSamplesJwt.Data;
using AspNetCoreSamplesJwt.Extensions;
using AspNetCoreSamplesJwt.Models;
using AspNetCoreSamplesJwt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreSamplesJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwt;

        public AuthController(ApplicationDbContext context, IJwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("signup"), AllowAnonymous]
        public async Task<ActionResult<dynamic>> OnSignUpAsync([FromBody] UserAccount account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else if (await _context.UserAccounts.AnyAsync(u => u.Email == account.Email))
            {
                ModelState.AddModelError("Error", "This email address is already in use.");
                return BadRequest(ModelState);
            }

            account.Password = StringExtension.GetKeyDerivationHash(account.Password, Encoding.UTF8.GetBytes(account.Email));
            _context.UserAccounts.Add(account);
            await _context.SaveChangesAsync();

            return StatusCode((int)HttpStatusCode.Created, account);
        }

        [HttpPost("signin"), AllowAnonymous]
        public async Task<ActionResult<dynamic>> OnSignInAsync([FromBody] UserCredentials account)
        {
            async Task<IJwtAccessTokenData> ProcessJwtTokensAsync(IUserAccount user, IJwtRefreshTokenData refreshTokenDataToRemove = null)
            {
                if (!string.IsNullOrWhiteSpace(account.RefreshToken))
                {
                    _context.JwtRefreshTokens.Remove(refreshTokenDataToRemove as JwtRefreshTokenData);
                }

                var (accessTokenData, refreshTokenData) = _jwt.GetJwtTokens(user);
                _context.JwtRefreshTokens.Add(refreshTokenData as JwtRefreshTokenData);
                await _context.SaveChangesAsync();

                return accessTokenData;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(account.RefreshToken))
                {
                    if (await _context.JwtRefreshTokens.FindAsync(account.RefreshToken) is IJwtRefreshTokenData refreshTokenData)
                    {
                        if (await _context.UserAccounts.FindAsync(refreshTokenData.Subject) is IUserAccount user)
                        {
                            if (refreshTokenData.ExpiresAt < DateTime.UtcNow)
                            {
                                return Ok(await ProcessJwtTokensAsync(user, refreshTokenData));
                            }
                            else
                            {
                                throw new ArgumentException("Expired token.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Invalid token.");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Token not founded.");
                    }
                }
                else
                {
                    account.Password = StringExtension.GetKeyDerivationHash(account.Password, Encoding.UTF8.GetBytes(account.Email));

                    if (await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == account.Email && u.Password == account.Password) is UserAccount user)
                    {
                        _context.JwtRefreshTokens.RemoveRange(await _context.JwtRefreshTokens.Where(r => r.Subject == user.Guid).ToListAsync());
                        return Ok(await ProcessJwtTokensAsync(user));
                    }
                    else
                    {
                        throw new NullReferenceException("User account not found with the provided credentials.");
                    }
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Errors", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet("all/accounts"), Authorize(Roles = "admin")]
        public async Task<ActionResult<dynamic>> OnGetAllUsersAccountsAsync()
        {
            return await _context.UserAccounts.ToListAsync();
        }

        [HttpGet("all/refreshtokens"), Authorize(Roles = "admin")]
        public async Task<ActionResult<dynamic>> OnGetAllJwtRefreshTokensDataAsync()
        {
            return await _context.JwtRefreshTokens.ToListAsync();
        }
    }
}