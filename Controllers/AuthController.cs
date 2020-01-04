using AspNetCoreSamplesJwt.Contracts;
using AspNetCoreSamplesJwt.Data;
using AspNetCoreSamplesJwt.Extensions;
using AspNetCoreSamplesJwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
            _context.Add(account);
            await _context.SaveChangesAsync();

            return StatusCode((int)HttpStatusCode.Created, account);
        }

        [HttpPost("signin"), AllowAnonymous]
        public async Task<ActionResult<dynamic>> OnSignInAsync([FromBody] UserAccount account)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(account.Email))
                {
                    throw new ArgumentNullException(nameof(account.Email));
                }
                else if (string.IsNullOrWhiteSpace(account.Password))
                {
                    throw new ArgumentNullException(nameof(account.Password));
                }

                account.Password = StringExtension.GetKeyDerivationHash(account.Password, Encoding.UTF8.GetBytes(account.Email));

                if (await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == account.Email && u.Password == account.Password) is UserAccount user)
                {
                    return Ok(_jwt.GetJwtToken(user));
                }
                else
                {
                    throw new NullReferenceException("User account not found with the provided credentials.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet("all"), Authorize(Roles = "admin")]
        public async Task<ActionResult<dynamic>> OnGetAllUsersAccountsAsync()
        {
            return await _context.UserAccounts.ToListAsync();
        }
    }
}