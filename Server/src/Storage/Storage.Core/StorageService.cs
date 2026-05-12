using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Core;
using Storage.Database;

namespace Storage;

internal sealed class StorageService(IServiceScopeFactory factory, ILogger<StorageService> logger)
    : EventSyncBackgroundService<StorageService, StorageDbContext>(factory, logger);
