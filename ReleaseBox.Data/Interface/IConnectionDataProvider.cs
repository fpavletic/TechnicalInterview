namespace ReleaseBox.Core.Data.Interface;

public interface IConnectionDataProvider
{
    public string GetDataSource();
    
    public string GetConnectionString();
}