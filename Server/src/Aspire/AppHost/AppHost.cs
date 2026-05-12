var builder = DistributedApplication.CreateBuilder(args);

var domainConnection = builder.AddConnectionString("Domain");
var queryConnection = builder.AddConnectionString("Query");
var storageConnection = builder.AddConnectionString("Storage");

var silo = builder.AddProject<Projects.Domain_Silo>("Domain-Silo").WithReference(domainConnection);

builder
    .AddProject<Projects.Query_Silo>("Query-Silo")
    .WithReference(queryConnection)
    .WithReference(domainConnection)
    .WaitFor(silo)
    .WithReplicas(3);

builder
    .AddProject<Projects.Storage_Silo>("Storage-Silo")
    .WithReference(domainConnection)
    .WithReference(storageConnection)
    .WaitFor(silo);

builder.Build().Run();
