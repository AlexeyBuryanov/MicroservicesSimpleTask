using Microsoft.AspNetCore.Mvc;
using PermissionsService.Data;
using PermissionsService.Data.Repositories;
using PermissionsService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using PermissionsService.Data.Repositories.Base;

namespace PermissionsService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly PermissionRepository _permissionsRepository;

        public PermissionsController(IRepositoryAsync<Permission> repository)
        {
            _permissionsRepository = repository as PermissionRepository;
        }

        // GET Permissions/list
        [HttpGet("list")]
        public async Task<IEnumerable<Permission>> Get()
        {
            return await _permissionsRepository.GetAllAsync();
        }

        // GET Permissions/list/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpGet("list/{userId}")]
        public async Task<IEnumerable<Permission>> GetForUser(string userId)
        {
            return await _permissionsRepository.GetUserPermissionsAsync(userId);
        }

        // POST Permissions/update/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpPost("update/{permissionId}")]
        public async Task<IActionResult> Post(string permissionId, [FromBody] Permission newOrUpdatedPermission)
        {
            if (newOrUpdatedPermission == null || string.IsNullOrWhiteSpace(permissionId))
            {
                return BadRequest();
            } // if

            var permission = await _permissionsRepository.AddOrReplaceAsync(permissionId: permissionId, item: newOrUpdatedPermission);
            if (permission != null)
            {
                return Ok(permission);
            }

            return NoContent();
        }

        // POST Permissions/assign/f1d80247-b360-483d-bef4-174f556cbdac/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpPost("assign/{permissionId}/{userId}")]
        public async Task<IActionResult> PostAssign(string permissionId, string userId)
        {
            if (string.IsNullOrWhiteSpace(permissionId) || string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            } // if

            var isOk = await _permissionsRepository.AssignPermissionAsync(permissionId: permissionId, userId: userId);

            return Ok(isOk);
        }

        // POST Permissions/unassign/f1d80247-b360-483d-bef4-174f556cbdac/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpPost("unassign/{permissionId}/{userId}")]
        public async Task<IActionResult> PostUnassign(string permissionId, string userId)
        {
            if (string.IsNullOrWhiteSpace(permissionId) || string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            } // if

            var isOk = await _permissionsRepository.UnassignPermissionAsync(permissionId: permissionId, userId: userId);

            return Ok(isOk);
        }

        // DELETE Permissions/delete/f1d80247-b360-483d-bef4-174f556cbdac
        [HttpDelete("delete/{permissionId}")]
        public async Task<IActionResult> Delete(string permissionId)
        {
            var permissions = await _permissionsRepository.GetAllAsync() as List<Permission>;

            if (permissions != null && !permissions.Exists(permission => permission.PermissionId == permissionId))
                return NotFound();
            {
                await _permissionsRepository.DeleteAsync(permissionId: permissionId);

                var okPermission = permissions?.Find(permission => permission.PermissionId == permissionId);
                if (okPermission != null)
                {
                    return Ok(okPermission);
                }

                return NotFound();
            }
        } // Delete
    }
}
