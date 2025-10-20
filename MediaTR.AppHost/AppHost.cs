var builder = DistributedApplication.CreateBuilder(args);

// Add databases
var mongodb = builder.AddMongoDB("mongodb")
    .WithMongoExpress();

var mongoDatabase = mongodb.AddDatabase("MediaTRDb");

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

var sqlserver = builder.AddSqlServer("sqlserver");
var reportingDatabase = sqlserver.AddDatabase("MediaTRReporting");
var outboxDatabase = sqlserver.AddDatabase("MediaTROutbox");

// Add API Service
var apiService = builder.AddProject<Projects.MediaTR_ApiService>("mediatr-api")
    .WithReference(mongoDatabase)
    .WithReference(redis)
    .WithReference(reportingDatabase)
    .WithReference(outboxDatabase);

builder.Build().Run();
