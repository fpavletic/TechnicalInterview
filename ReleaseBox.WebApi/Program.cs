using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Interfaces;
using ReleaseBox.Core.Services;
using ReleaseBox.Data.Sqlite;
using ReleaseBox.Data.Sqlite.ConnectionDataProvider;
using ReleaseBox.Data.Sqlite.Repositories;
using Serilog;
using Constants = ReleaseBox.Util.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Setup logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(Constants.FileStorageDirectory, "ReleaseBox.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

//Setup database
IConnectionDataProvider ConnectionDataProvider(IServiceProvider _) => new ConstantOnDiskConnectionDataProvider(Constants.FileStorageDirectory);

builder.Services.AddSingleton(ConnectionDataProvider);
var connectionDataProvider = ConnectionDataProvider(null!);
var databaseDirectoryPath = Path.GetDirectoryName(connectionDataProvider.GetDataSource()); 
if (!Directory.Exists(databaseDirectoryPath))
{
    Directory.CreateDirectory(databaseDirectoryPath!);
}
var inMemoryDatabaseInitializer = new DatabaseInitializer(connectionDataProvider);
inMemoryDatabaseInitializer.Initialize();
inMemoryDatabaseInitializer.Dispose();

//Repository services
builder.Services.AddScoped<IDirectoryRepository, DirectoryRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileSystemEntryRepository, FileSystemEntryRepository>();
//Logic services
builder.Services.AddScoped<IDirectoryService, DirectoryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileSystemEntryService, FileSystemEntryService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


app.Run();