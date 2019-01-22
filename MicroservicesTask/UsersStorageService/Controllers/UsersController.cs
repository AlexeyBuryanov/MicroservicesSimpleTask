using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UsersStorageService.Data.Repositories.Base;
using UsersStorageService.Models;

namespace UsersStorageService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IRepositoryAsync<User> _usersRepository;

        public UsersController(IRepositoryAsync<User> repository)
        {
            _usersRepository = repository;
        }

        // GET Users/list
        [HttpGet("list")]
        public async Task<IEnumerable<User>> Get()
        {
            return await _usersRepository.GetAllAsync();
        }

        // POST Users/update/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpPost("update/{userId}")]
        public async Task<IActionResult> Post(string userId, [FromBody] User newOrUpdatedUser)
        {
            if (newOrUpdatedUser == null || string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            } // if

            var user = await _usersRepository.AddOrReplaceAsync(id: userId, item: newOrUpdatedUser);
            if (user != null)
            {
                return Ok(user);
            }

            return NoContent();
        }

        // DELETE Users/delete/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            var users = await _usersRepository.GetAllAsync() as List<User>;

            if (users != null && !users.Exists(user => user.UserId == userId))
                return NotFound();
            {
                await _usersRepository.DeleteAsync(id: userId);

                var okUser = users?.Find(user => user.UserId == userId);

                if (okUser != null)
                {
                    return Ok(okUser);
                }

                return NotFound();
            }
        } // Delete
    }
}
