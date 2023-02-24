using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.OcrService
{
	public interface IOcrService
	{
		Task<AnalyzeResults> GetOcrResultsAsync(string blobName, Guid correlationId);
	}
}

