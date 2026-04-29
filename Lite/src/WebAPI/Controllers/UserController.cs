using System.ComponentModel.DataAnnotations;
using Domain.Shared;
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


    public readonly record struct UpdateProfileRequest(Nickname Nickname, Biography Biography);

    [Authorize]
    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] [Required] UpdateProfileRequest request,
        CancellationToken cancellationToken
    )
    {
        UpdateProfileCommand command = new(request.Nickname, request.Biography, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("avatar")]
    [RequestSizeLimit(ImageFile.MaxBytes)]
    public async Task<IActionResult> UpdateAvatar(
        [FromForm] [FileValidator(ImageFile.MaxBytes)] [Required] IFormFile file,
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
    [RequestSizeLimit(ImageFile.MaxBytes)]
    public async Task<IActionResult> UpdateHeader(
        [FromForm] [FileValidator(ImageFile.MaxBytes)] [Required] IFormFile file,
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
