using System.Net;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class OperationResult
    {
        public OperationResult(string etag, HttpStatusCode statusCode)
        {
            Require.NotNull(etag, "etag");

            ETag = etag;
            StatusCode = statusCode;
        }

        public string ETag { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
