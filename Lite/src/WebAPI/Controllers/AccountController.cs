using System.ComponentModel.DataAnnotations;
using Application.UserServices.Queries;
using AspNet.Security.OAuth.GitHub;
using Domain.Shared;
using Domain.UserAggregate.Commands;
using Domain.UserAggregate.Commands.OAuth.GitHub;
using Domain.UserAggregate.UserEntity;
using Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Utilities;

namespace WebAPI.Controllers;

[Route("api/account")]
[ApiController]
public sealed class AccountController(IMediator mediator) : ControllerBase
{
    public readonly record struct RegisterRequest(
        Username Username,
        Nickname Nickname,
        Email Email,
        PasswordInput Password,
        RegistryCode Code
    );

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] [Required] RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        RegisterCommand command = new(
            request.Username,
            request.Nickname,
            request.Email,
            request.Password,
            request.Code
        );
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("register/code")]
    public async Task<IActionResult> SendRegistryCode(
        [FromQuery] [Required] Email email,
        CancellationToken cancellationToken
    )
    {
        SendRegistryCodeCommand command = new(email);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    public readonly record struct LoginRequest(Username Username, PasswordInput Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] [Required] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        LoginCommand command = new(request.Username, request.Password);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    public readonly record struct RefreshTokenRequest(RefreshToken RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAccessToken(
        [FromBody] [Required] RefreshTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        RefreshTokenCommand command = new(request.RefreshToken);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    public readonly record struct ResetPasswordRequest(
        PasswordInput OldPassword,
        PasswordInput NewPassword
    );

    [Authorize]
    [HttpPost("reset/password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] [Required] ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        ResetPasswordCommand command = new(request.OldPassword, request.NewPassword, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    public readonly record struct ResetUsernameRequest(Username Username);

    [Authorize]
    [HttpPost("reset/username")]
    public async Task<IActionResult> ResetUsername(
        [FromBody] [Required] ResetUsernameRequest request,
        CancellationToken cancellationToken
    )
    {
        ResetUsernameCommand command = new(request.Username, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("username/check")]
    public async Task<IActionResult> CheckUsername(
        [FromQuery] [Required] [Length(Username.MinLength, Username.MaxLength)] string username,
        CancellationToken cancellationToken
    )
    {
        var query = new UsernameExistenceQuery(username.Bind<Username>());
        var result = await mediator.Send(query, cancellationToken);
        return Ok(!result.IsExist);
    }

    #region GitHub OAuth

    const string GitHubScheme = GitHubAuthenticationDefaults.AuthenticationScheme;

    [HttpGet($"oauth/{GitHubScheme}")]
    public async Task<IActionResult> GitHubOAuth()
    {
        var props = new AuthenticationProperties()
        {
            RedirectUri = Url.Action(nameof(GitHubOAuthRedirect)),
        };
        return Challenge(props, GitHubScheme);
    }

    [Authorize(AuthPolicies.OAuth)]
    [HttpGet($"oauth/{GitHubScheme}/redirect")]
    public async Task<IActionResult> GitHubOAuthRedirect(CancellationToken cancellationToken)
    {
        if (User.TryFetchId(out long id) is false)
            return Unauthorized();

        var command = new GitHubLoginCommand(new(id));
        var token = await mediator.Send(command, cancellationToken);

        return token is null ? NoContent() : Ok(token.Value);
    }

    [Authorize]
    [HttpGet($"oauth/{GitHubScheme}/link")]
    public async Task<IActionResult> GitHubOAuthLink(CancellationToken cancellationToken)
    {
        if (User.TryFetchId(out long userId) is false)
            return Unauthorized();

        var result = await HttpContext.AuthenticateAsync(GitHubScheme);
        if (
            result is not { Succeeded: true, Principal: { } user }
            || user.TryFetchId(out long githubId) is false
        )
            return Unauthorized();

        var command = new GitHubLinkCommand(new(userId), new(githubId));
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    #endregion
}
