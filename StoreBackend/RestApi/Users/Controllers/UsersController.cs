using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using System.Security.Claims;
using UseValidator;

namespace RestApi.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService, IUserResponseFormatter responseFormatter, IAuthorizationService authorizationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] UserFilters userFilters)
        {
            var result = await userService.Search(userFilters);
            return Ok(responseFormatter.Many(result.Data, result.TotalCount));
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateUserRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            CreateUserParams createParams = new()
            {
                Username = request.Username,
                Password = request.Password,
                Role = request.Role,
                CreatedById = this.GetUserId()
            };

            var user = await userService.Create(createParams);
            var result = responseFormatter.One(user);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
        }


        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await userService.FindById(id);

            //var userRole = User.FindFirstValue(ClaimTypes.Role);

            //var isAdmin = userRole == "Admin";

            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (user.Id.ToString() != userId && !isAdmin)
            //    return Forbid();

            var authResult = await authorizationService.AuthorizeAsync(User,id, "UserOwnerOrAdminPolicy");

            if (!authResult.Succeeded)
                return Forbid();

            return user is null
                ? NotFound()
                : Ok(responseFormatter.One(user));
        }

        [HttpPatch("{userId}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateUserRequestValidator))]
        public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserRequest request)
        {
            var updatedUser = await userService.Update(new UpdateUserParams
            {
                Id = userId,    
                Username = request.Username,
                Password = request.Password,
                Role = request.Role,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedUser));
        }
    }
}
