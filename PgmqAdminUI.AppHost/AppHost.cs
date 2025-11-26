var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL with PGMQ extension (using Tembo's pg18-pgmq image)
var postgres = builder.AddPostgres("postgres")
    .WithImage("quay.io/tembo/pg18-pgmq", "v1.7.0")
    .WithPgAdmin()
    .AddDatabase("pgmq");

// Blazor Admin UI
builder.AddProject<Projects.PgmqAdminUI>("pgmq-admin-ui")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();
