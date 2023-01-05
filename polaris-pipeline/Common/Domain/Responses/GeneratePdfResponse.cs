namespace Common.Domain.Responses
{
    public class GeneratePdfResponse
    {
        public GeneratePdfResponse(string blobName)
        {
            BlobName = blobName;
        }
        
        public string BlobName { get; set; }

        public bool AlreadyProcessed { get; set; }
    }
}