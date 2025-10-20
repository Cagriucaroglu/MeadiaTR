var builder = DistributedApplication.CreateBuilder(args);

// Get persistent data path from environment variable
var projectDataPath = Environment.GetEnvironmentVariable("PROJECT_DATA")
    ?? throw new InvalidOperationException("PROJECT_DATA environment variable is not set");

// Add databases
var mongodb = builder.AddMongoDB("mongodb")
    .WithMongoExpress()
    .WithDataVolume(Path.Combine(projectDataPath, "MediaTR", "mongodb"));

var mongoDatabase = mongodb.AddDatabase("MediaTRDb");

var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataVolume(Path.Combine(projectDataPath, "MediaTR", "redis"));

var sqlserver = builder.AddSqlServer("sqlserver")
    .WithDataVolume(Path.Combine(projectDataPath, "MediaTR", "sqlserver"));

var reportingDatabase = sqlserver.AddDatabase("MediaTRReporting");
var outboxDatabase = sqlserver.AddDatabase("MediaTROutbox");

// Add API Service
var apiService = builder.AddProject<Projects.MediaTR_ApiService>("mediatr-api")
    .WithReference(mongoDatabase)
    .WithReference(redis)
    .WithReference(reportingDatabase)
    .WithReference(outboxDatabase);

builder.Build().Run();
