using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Query.Database;
using Shared.Core;

namespace Query;

public sealed class QueryService(IServiceScopeFactory factory, ILogger<QueryService> logger)
    : EventSyncBackgroundService<QueryService, QueryDbContext>(factory, logger);
