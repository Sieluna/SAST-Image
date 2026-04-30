using System.ComponentModel.DataAnnotations;
using Domain.CategoryAggregate.CategoryEntity;
using Domain.CategoryAggregate.Commands;
using Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Query.Categories.Queries;
using WebAPI.Utilities.Attributes;

namespace WebAPI.Controllers;

[Route("api/categories")]
[ApiController]
[ResponseCache(Duration = 10, Location = ResponseCacheLocation.Client)]
public sealed class CategoryController(IMediator mediator) : AdvancedController
{
    #region [Command/Post]

    public readonly record struct CreateCategoryRequest(
        [property: Required] CategoryName Name,
        [property: Required] CategoryDescription Description
    );

    [HttpPost]
    [Authorize(AuthPolicies.Admin)]
    [EndpointName("Create Category")]
    [EndpointDescription("Create a new category.")]
    [MaybeConflict]
    public async Task<Ok<CategoryId>> Create(
        [FromBody, Required] CreateCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        CreateCategoryCommand command = new(request.Name, request.Description, User);
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    public readonly record struct UpdateCategoryNameRequest([property: Required] CategoryName Name);

    [HttpPost("{id:long}/name")]
    [Authorize(AuthPolicies.Admin)]
    [EndpointName("Update Category Name")]
    [EndpointDescription("Update a category's name.")]
    [MaybeConflict]
    public async Task<NoContent> UpdateName(
        [FromRoute] CategoryId id,
        [FromBody, Required] UpdateCategoryNameRequest request,
        CancellationToken cancellationToken
    )
    {
        UpdateCategoryNameCommand command = new(id, request.Name, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    public readonly record struct UpdateCategoryDescriptionRequest(
        [property: Required] CategoryDescription Description
    );

    [HttpPost("{id:long}/description")]
    [Authorize(AuthPolicies.Admin)]
    [EndpointName("Update Category Description")]
    [EndpointDescription("Update a category's description.")]
    public async Task<NoContent> UpdateDescription(
        [FromRoute] CategoryId id,
        [FromBody, Required] UpdateCategoryDescriptionRequest request,
        CancellationToken cancellationToken
    )
    {
        UpdateCategoryDescriptionCommand command = new(id, request.Description, User);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    //[HttpPost("{id:long}/remove")]
    //public async Task<IActionResult> Remove(
    //    [FromRoute] long id,
    //    CancellationToken cancellationToken
    //)
    //{
    //    var command = new RemoveCategoryCommand(new(id), User);
    //    await mediator.Send(command, cancellationToken);
    //    return NoContent();
    //}

    #endregion

    #region [Query/Get]

    [HttpGet]
    [EndpointName("Get Categories")]
    [EndpointDescription("Get all categories.")]
    public async Task<Ok<CategoryDto[]>> Get(CancellationToken cancellationToken)
    {
        CategoriesQuery query = new();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    //[HttpGet("{id:long}")]
    //public async Task<IActionResult> Get([FromRoute] long id, CancellationToken cancellationToken)
    //{
    //    var query = new GetCategoryQuery(new(id));
    //    var result = await mediator.Send(query, cancellationToken);
    //    return Ok(result);
    //}

    #endregion
}
