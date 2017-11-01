namespace QP8.Integration.Tests.Infrastructure
{
    internal class ContentDataItem
    {
        public int FieldId { get; set; }

        public string Data { get; set; }

        public string BlobData { get; set; }

        public override string ToString() => new { FieldId, Data, BlobData }.ToString();
    }
}
