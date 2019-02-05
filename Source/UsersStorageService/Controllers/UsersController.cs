using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public UsersController(
            IRepositoryAsync<User> repository,
            ILogger<UsersController> logger)
        {
            _usersRepository = repository;
            _logger = logger;
        }

        // GET Users/list
        [HttpGet("list")]
        public async Task<IEnumerable<User>> Get()
        {
            _logger.LogInformation("--- GET Users/list");

            return await _usersRepository.GetAllAsync();
        }

        // POST Users/update/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpPost("update/{userId}")]
        public async Task<IActionResult> Post(string userId, [FromBody] User newOrUpdatedUser)
        {
            _logger.LogInformation($"--- POST Users/update/{userId}");

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
            _logger.LogInformation($"--- DELETE Users/delete/{userId}");

            var users = await _usersRepository.GetAllAsync() as List<User>;

            if (users != null && !users.Exists(user => user.UserId == userId))
                return NotFound();
            {
                var okUser = users?.Find(user => user.UserId == userId);

                await _usersRepository.DeleteAsync(id: userId);

                if (okUser != null)
                {
                    return Ok(okUser);
                }

                return NotFound();
            }
        } // Delete
    }
}
