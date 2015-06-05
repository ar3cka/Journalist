using System;
using System.Net;
using System.Runtime.Serialization;
using Journalist.Extensions;

namespace Journalist.WindowsAzure.Storage.Tables
{
    [Serializable]
    public sealed class BatchOperationException : Exception
    {
        public BatchOperationException()
        {
        }

        public BatchOperationException(string message) : base(message)
        {
        }

        public BatchOperationException(string message, Exception inner) : base(message, inner)
        {
        }

        public BatchOperationException(int operationBatchNumber, HttpStatusCode statusCode, string operationErrorCode, Exception inner)
            : base("Batch operation '{0}' failed with error ('{1}', '{2}').".FormatString(operationBatchNumber, statusCode, operationErrorCode), inner)
        {
            OperationBatchNumber = operationBatchNumber;
            HttpStatusCode = statusCode;
            OperationErrorCode = operationErrorCode;
        }

        private BatchOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            OperationErrorCode = info.GetString("operationErrorCode");
            HttpStatusCode = (HttpStatusCode)info.GetInt32("HttpStatusCode");
            OperationBatchNumber = info.GetInt32("OperationBatchNumber");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("operationErrorCode", OperationErrorCode);
            info.AddValue("HttpStatusCode", (int) HttpStatusCode);
            info.AddValue("OperationBatchNumber", OperationErrorCode);
        }

        public string OperationErrorCode { get; private set; }

        public HttpStatusCode HttpStatusCode { get; private set; }

        public int OperationBatchNumber { get; private set; }
    }
}
