using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace coordinator.Services.OcrResultsService
{
    public class OcrResultsService : IOcrResultsService
    {
        private const int CharacterLimit = 1000;

        public OcrResult GetDocumentText(AnalyzeResults analyzeResults)
        {
            var pages = new List<PageResult>();

            foreach (var result in analyzeResults.ReadResults)
            {
                var pageResult = new PageResult(result);

                pages.Add(pageResult);
            }

            return new OcrResult(pages);
        }
    }

    public class OcrResult
    {
        // Chunk documents here?

        public OcrResult(IList<PageResult> pages)
        {
            Text = GetPageText(pages);
        }

        public IList<PageResult> Pages { get; set; }
        public int PageCount => Pages.Count;
        public int LineCount => Pages.Sum(x => x.LineCount);
        public int WordCount => Pages.Sum(x => x.WordCount);
        public int TextLength => Text.Length;
        public string Text { get; set; }

        private static string GetPageText(IList<PageResult> pageResults)
        {
            var sb = new StringBuilder();
            foreach (var page in pageResults)
            {
                sb.AppendFormat($"{page.Text} ");
            }

            return sb.ToString().TrimEnd();
        }
    }

    public class PageResult
    {
        public PageResult(ReadResult readResult)
        {
            PageNumber = readResult.Page;
            LineCount = readResult.Lines.Count;
            WordCount = readResult.Lines.Sum(y => y.Words.Count);
            AddLines(readResult.Lines);
            Text = GetPageText();
        }

        public IList<OcrLine> Lines { get; set; } = new List<OcrLine>();
        public int PageNumber { get; set; }
        public int LineCount { get; set; }
        public int WordCount { get; set; }
        public string Text { get; set; }

        private void AddLines(IList<Line> lines)
        {
            foreach (var line in lines)
            {
                AddLine(line.Text);
            }
        }

        public void AddLine(string text)
        {
            var previousLine = Lines.LastOrDefault();
            var ocrLine = new OcrLine(text, previousLine);
            Lines.Add(ocrLine);
        }

        public string GetPageText()
        {
            var sb = new StringBuilder();
            foreach (var line in Lines)
            {
                sb.AppendFormat($"{line.Text}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    public class OcrLine
    {
        public OcrLine(string text, OcrLine previousLine)
        {
            Text = $"{text} ";
            SetOffsetRange(previousLine);
        }

        public string Text { get; set; }
        public int Length => Text.Length;
        public (int Min, int Max) OffsetRange { get; protected set; }

        public void SetOffsetRange(OcrLine previousLine)
        {
            if (previousLine == null)
                OffsetRange = (0, Text.Length);
            else
            {
                var accumulativeOffset = previousLine.OffsetRange.Max;
                OffsetRange = (accumulativeOffset + 1, accumulativeOffset + Length);
            }
        }
    }
}