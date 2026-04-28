using System.ComponentModel.DataAnnotations;
using Domain.UserAggregate.Commands.Profile;
using Domain.UserAggregate.UserEntity;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Query.Users.Queries;
using Storage.Users.Queries;
using WebAPI.Utilities;
using WebAPI.Utilities.Attributes;

namespace WebAPI.Controllers;

[Route("api/users")]
[ApiController]
public sealed class UserController(IMediator mediator) : ControllerBase
{
    #region [Command/Post]


    public readonly record struct UpdateNicknameRequest(Nickname Nickname);

    [Authorize]
    [HttpPost("nickname")]
    public async Task<IActionResult> UpdateNickname(
        [FromBody] [Required] UpdateNicknameRequest request,
        CancellationToken cancellationToken
    )
    {
        UpdateNicknameCommand command = new(request.Nickname, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    public readonly record struct UpdateBiographyRequest(Biography Biography);

    [Authorize]
    [HttpPost("biography")]
    public async Task<IActionResult> UpdateBiography(
        [FromBody] [Required] UpdateBiographyRequest request,
        CancellationToken cancellationToken
    )
    {
        UpdateBiographyCommand command = new(request.Biography, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("avatar")]
    public async Task<IActionResult> UpdateAvatar(
        [FromForm] [FileValidator(0, 3)] [Required] IFormFile file,
        CancellationToken cancellationToken
    )
    {
        var avatar = await file.GetAsync(cancellationToken);
        UpdateAvatarCommand command = new(avatar, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("header")]
    public async Task<IActionResult> UpdateHeader(
        [FromForm] [FileValidator(0, 10)] [Required] IFormFile file,
        CancellationToken cancellationToken
    )
    {
        var header = await file.GetAsync(cancellationToken);
        UpdateHeaderCommand command = new(header, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    #endregion

    #region [Query/Get]

    [HttpGet("{id:long}/avatar")]
    public async Task<IActionResult> GetAvatar(
        [FromRoute] UserId id,
        CancellationToken cancellationToken
    )
    {
        var query = new UserAvatarQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return this.ImageOrNotFound(result);
    }

    [HttpGet("{id:long}/header")]
    public async Task<IActionResult> GetHeader(
        [FromRoute] UserId id,
        CancellationToken cancellationToken
    )
    {
        var query = new UserHeaderQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return this.ImageOrNotFound(result);
    }

    [HttpGet("{id:long}/profile")]
    public async Task<IActionResult> GetProfileInfo(
        [FromRoute] UserId id,
        CancellationToken cancellationToken
    )
    {
        var query = new UserProfileQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return this.DataOrNotFound(result);
    }

    #endregion
}
