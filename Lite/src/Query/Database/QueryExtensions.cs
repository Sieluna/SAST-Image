using System.Linq.Expressions;
using Domain.AlbumAggregate.AlbumEntity;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Query.Albums;
using Query.Images;

namespace Query.Database;

public static class QueryExtensions
{
    public static IQueryable<ImageModel> WhereIsAccessible(
        this IQueryable<ImageModel> images,
        long userId,
        bool isAuthenticated,
        bool isAdmin
    )
    {
        return images.Where(i =>
            i.AccessLevel >= AccessLevelValue.PublicReadOnly
            || i.AccessLevel >= AccessLevelValue.AuthReadOnly && isAuthenticated
            || i.AccessLevel == AccessLevelValue.Private
                && (i.AuthorId == userId || i.Collaborators.Contains(userId) || isAdmin)
        );
    }

    public static IQueryable<AlbumModel> WhereIsAccessible(
        this IQueryable<AlbumModel> albums,
        long userId,
        bool isAuthenticated,
        bool isAdmin
    )
    {
        return albums.Where(a =>
            a.AccessLevel >= AccessLevelValue.PublicReadOnly
            || a.AccessLevel >= AccessLevelValue.AuthReadOnly && isAuthenticated
            || a.AccessLevel == AccessLevelValue.Private
                && (a.AuthorId == userId || a.Collaborators.Contains(userId) || isAdmin)
        );
    }

    extension<TEntity>(IQueryable<TEntity> entities)
        where TEntity : class
    {
        public async Task<TEntity> GetAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken
        )
        {
            return await entities.FirstOrDefaultAsync(predicate, cancellationToken)
                ?? throw new EntityNotFoundException();
        }
    }
}
