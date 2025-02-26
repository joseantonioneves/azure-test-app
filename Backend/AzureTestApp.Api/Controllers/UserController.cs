using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AzureTestApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            var userName = User.Identity?.Name;
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            var userInfo = new
            {
                Name = userName,
                TenantId = tenantId
            };

            return Ok(userInfo);
        }

        [HttpGet("tenant-info")]
        public IActionResult GetTenantInfo()
        {
            var tenantId = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            var tenantInfo = new
            {
                TenantId = tenantId,
                RecentLoginAttempts = new[] { "2023-10-01", "2023-10-02" },
                Users = new[] { "user1", "user2" },
                Groups = new[] { "group1", "group2" }
            };

            return Ok(tenantInfo);
        }
    }
}