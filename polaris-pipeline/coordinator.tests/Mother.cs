using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace coordinator.tests
{
    internal static class Mother
    {
        public static Func<Line> OcrLine1 { get; } = () => new(null, "This is line 1",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "1", 1)
                    });

        public static Func<Line> OcrLine2 { get; } = () => new(null, "This is line 2",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "2", 1)
                    });

        public static Func<Line> OcrLine3 { get; } = () => new(null, "This is the third line of OCR analysis",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "the", 1),
                        new(null, "third", 1),
                        new(null, "line", 1),
                        new(null, "of", 1),
                        new(null, "OCR", 1),
                        new(null, "analysis", 1)
                    });

        public static Func<IEnumerable<Line>> OcrLines { get; } = () => new[]
        {
            OcrLine1(),
            OcrLine2(),
            OcrLine3()
        };
    }
}