var builder = DistributedApplication.CreateBuilder(args);

// Add parameters for database credentials (from user secrets)
var sqlPassword = builder.AddParameter("mediatr-sql-password", secret: true);
var mongoUserName = builder.AddParameter("mediatr-mongo-username");
var mongoPassword = builder.AddParameter("mediatr-mongo-password", secret: true);

// Get persistent data path from PROJECT_DATA environment variable
var projectDataPath = Environment.GetEnvironmentVariable("PROJECT_DATA");

if (string.IsNullOrEmpty(projectDataPath))
{
    throw new InvalidOperationException(
        "PROJECT_DATA environment variable is not set. " +
        "Please set PROJECT_DATA environment variable to specify the data directory.");
}

// SQL Server with persistent storage and credentials
var sqlserver = builder.AddSqlServer("sqlserver", sqlPassword)
    .WithDataBindMount(Path.Combine(projectDataPath, "MediaTR", "SqlData"))
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsConnectionString();

var reportingDatabase = sqlserver.AddDatabase("MediaTRReporting");
var auditTrailDatabase = sqlserver.AddDatabase("MediaTRAuditTrail");

// MongoDB with persistent storage and credentials
var mongodb = builder.AddMongoDB("mongodb", userName: mongoUserName, password: mongoPassword)
    .WithMongoExpress()
    .WithDataBindMount(Path.Combine(projectDataPath, "MediaTR", "MongoData"))
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsConnectionString();

var mongoDatabase = mongodb.AddDatabase("MediaTRDb");

// Redis with persistent storage
var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataBindMount(Path.Combine(projectDataPath, "MediaTR", "RedisData"))
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsConnectionString();

// Add Migration Service (OptimatePlatform Init Container Pattern)
// This service will run migrations and then exit
var migrationService = builder.AddProject<Projects.MediaTR_MigrationService>("migration-service")
    .WithReference(auditTrailDatabase)
    .WaitFor(sqlserver);

// Add API Service with dependencies
// IMPORTANT: API waits for migration to complete before starting
var apiService = builder.AddProject<Projects.MediaTR_ApiService>("mediatr-api")
    .WithReference(mongoDatabase)
    .WithReference(redis)
    .WithReference(reportingDatabase)
    .WithReference(auditTrailDatabase)
    .WaitFor(sqlserver)
    .WaitFor(mongodb)
    .WaitFor(redis)
    .WaitForCompletion(migrationService);  // CRITICAL: Wait for migrations to complete

builder.Build().Run();
