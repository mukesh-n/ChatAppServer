using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using MyDotNetAPP.Models;
using MyDotNetAPP.Services;

namespace MyDotNetAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDbConnection _dbConnection;
        private UsersService _usersService;

        public UsersController(IDbConnection dbConnection, UsersService usersService)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _usersService = usersService;
        }



        [HttpGet("Get")]
        public async Task<ActionResult<ActionRes<Users>>> Get()
        {
            ActionRes<List<Users>> result = new ActionRes<List<Users>>();
            result.item = await _usersService.Get();
            return Ok(result);
        }

        [HttpPost("Select")]
        public async Task<ActionResult<ActionRes<Users>>> Select(ActionReq<UserSelectReq> req)
        {
            ActionRes<List<Users>> result = new ActionRes<List<Users>>();
            result.item = await _usersService.Select(req.item);
            return Ok(result);
        }

        [HttpPost("Insert")]
        public async Task<ActionResult<ActionRes<Users>>> Insert(ActionReq<Users> req)
        {
            ActionRes<Users> result = new ActionRes<Users>();
            result.item = await _usersService.Insert(req.item);
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<ActionResult<ActionRes<Users>>> Update(ActionReq<UserUpdateReq> req)
        {
            ActionRes<bool> result = new ActionRes<bool>();
            result.item = await _usersService.Update(req.item);
            return Ok(result);
        }


        [HttpPost("Login")]
        public async Task<ActionResult<ActionRes<UserLoginRes>>> Login(ActionReq<UserLoginReq> req)
        {
            ActionRes<UserLoginRes> result = new ActionRes<UserLoginRes>();
            result.item = await _usersService.Login(req.item);
            return Ok(result);
        }

        [HttpPost("SignUp")]
        public async Task<ActionResult<ActionRes<bool>>> UpdatePassword(ActionReq<UserSignUpReq> req)
        {
            ActionRes<bool> result = new ActionRes<bool>();
            await _usersService.Signup(req.item);
            result.item = true;
            return Ok(result);
        }
    }
}


