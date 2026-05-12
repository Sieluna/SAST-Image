using Domain.Api;
using Domain.Category;
using Mediator;
using Query.Categories.Queries;
using CategoryGrain = Domain.Category.ICategoryGrain;
using CategoryName = Domain.Category.CategoryName;
using CategoryDescription = Domain.Category.CategoryDescription;

namespace Interface.Endpoints;

internal static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories");

        group.MapGet("/", async (IMediator mediator) =>
        {
            return Results.Ok(await mediator.Send(new CategoriesQuery()));
        });

        group.MapPost("/", async (
            CreateCategoryRequest request,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<ICategoryGrain>(0);
            var id = await grain.Create(
                new CategoryName(request.Name),
                new CategoryDescription(request.Description));
            return Results.Ok(new CategoryResponse(id.Value, request.Name, request.Description));
        }).RequireAuthorization();

        group.MapPut("/{id:long}", async (
            long id,
            UpdateCategoryRequest request,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<ICategoryGrain>(id);
            await grain.Update(
                request.Name is not null ? new CategoryName(request.Name) : null,
                request.Description is not null ? new CategoryDescription(request.Description) : null);
            return Results.Ok();
        }).RequireAuthorization();

        group.MapDelete("/{id:long}", async (
            long id,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<ICategoryGrain>(id);
            await grain.Delete();
            return Results.Ok();
        }).RequireAuthorization();
    }
}
