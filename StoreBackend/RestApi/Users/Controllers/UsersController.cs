using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Users.Controllers
{
    /// <summary>
    /// Account administration. Managing somebody else's account is an Admin job, so every
    /// action here carries that role - except <see cref="GetById"/>, which opens up just
    /// far enough for somebody to read their own record back.
    /// </summary>
    /// <remarks>
    /// The role sits on the actions rather than on the controller on purpose: ASP.NET Core
    /// ANDs a controller-level <c>[Authorize]</c> with an action-level one, so an Admin
    /// requirement up here would quietly shut non-admins out of their own record.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService, IUserResponseFormatter responseFormatter, IAuthorizationService authorizationService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(UserListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] UserFilters userFilters)
        {
            var result = await userService.Search(userFilters);
            return Ok(responseFormatter.Many(result.Data, result.TotalCount));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [ProducesResponseType(typeof(object), 409)]
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

            // A name already taken leaves here as ResourceAlreadyExistsException and is
            // answered as a 409 by GlobalExceptionHandler, which every other broken rule
            // goes through too.
            var user = await userService.Create(createParams);

            var result = responseFormatter.One(user);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
        }


        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await userService.FindById(id);

            var authResult = await authorizationService.AuthorizeAsync(User, id, "UserOwnerOrAdminPolicy");

            if (!authResult.Succeeded)
                return Forbid();

            return user is null
                ? NotFound()
                : Ok(responseFormatter.One(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{userId}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var currentUserId = this.GetUserId();

            // Deleting the account you are signed in with would lock you out of the page you
            // did it from, so it is refused outright rather than half-handled.
            if (string.Equals(currentUserId, userId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "You cannot delete the account you are signed in with." });
            }

            var deleted = await userService.Delete(new DeleteUserParams
            {
                Id = userId,
                DeletedById = currentUserId
            });

            return deleted ? NoContent() : NotFound();
        }
    }
}
