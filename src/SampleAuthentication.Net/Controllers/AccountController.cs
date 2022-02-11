using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleAuthentication.Net.Models;
using SampleAuthentication.Net.Services;

namespace SampleAuthentication.Net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        public readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0).Select(k => ModelState[k].Errors[0].ErrorMessage);
                    return Ok(new AuthResult { Errors = errors, Successful = false, Token = null, UserId = null });
                }
                else
                {
                    var res = await _accountService.Register(model);
                    return Ok(res);
                }
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex);
            }
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var res = await _accountService.Login(model);
                return Ok(res);
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex);
            }
        }

    }
}
