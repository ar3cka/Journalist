namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ITableRequestOptions
    {
        ITableRequestOptions ReadFromSecondaryLocation();
    }
}