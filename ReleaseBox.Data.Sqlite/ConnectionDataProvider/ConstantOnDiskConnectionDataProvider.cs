using ReleaseBox.Core.Data.Interface;

namespace ReleaseBox.Data.Sqlite.ConnectionDataProvider;

public class ConstantOnDiskConnectionDataProvider : IConnectionDataProvider
{
    private const string DatabaseName = "ReleaseBox.sqlite";

    private readonly string _dataSource;
    private readonly string _connectionString;

    public ConstantOnDiskConnectionDataProvider(string directoryPath)
    {
        _dataSource = Path.Combine(directoryPath, DatabaseName);
        _connectionString = $"Data Source={_dataSource};Version=3;New=True;foreign keys=true;";
    }
    
    public string GetDataSource() => _dataSource;
    
    public string GetConnectionString() => _connectionString;
}