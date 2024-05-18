using System;
using System.Collections.Generic;
using coordinator.Domain;
using coordinator.Services.OcrResultsService;
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

        public static Func<Word> Word1 { get; } = () => new(new List<double?> { }, "Joint", 1);
        public static Func<Word> Word2 { get; } = () => new(new List<double?> { }, "NPCC", 1);
        public static Func<Word> Word3 { get; } = () => new(new List<double?> { }, "and", 1);
        public static Func<Word> Word4 { get; } = () => new(new List<double?> { 2.3283, 1.0196, 2.6279, 1.0196, 2.6279, 1.1326, 2.3283, 1.1326 }, "CPS", 1);
        public static Func<Word> Word5 { get; } = () => new(new List<double?> { }, "Evidence", 1);
        public static Func<Word> Word6 { get; } = () => new(new List<double?> { }, "Gathering", 1);
        public static Func<Word> Word7 { get; } = () => new(new List<double?> { }, "Checklist", 1);
        public static Func<Word> Word8 { get; } = () => new(new List<double?> { }, "-", 1);
        public static Func<Word> Word9 { get; } = () => new(new List<double?> { }, "For", 1);
        public static Func<Word> Word10 { get; } = () => new(new List<double?> { }, "Use", 1);
        public static Func<Word> Word11 { get; } = () => new(new List<double?> { }, "by", 1);
        public static Func<Word> Word12 { get; } = () => new(new List<double?> { 5.8473, 1.0214, 6.2805, 1.0214, 6.2805, 1.1325, 5.8473, 1.1325 }, "Police", 1);
        public static Func<Word> Word13 { get; } = () => new(new List<double?> { 6.3399, 1.0214, 6.8222, 1.0214, 6.8222, 1.1325, 6.3399, 1.1325 }, "Forces", 1);
        public static Func<Word> Word14 { get; } = () => new(new List<double?> { }, "and", 1);

        public static Func<Line> OcrLine4 { get; } = () => new(null, "Joint NPCC and CPS Evidence Gathering Checklist – For Use by Police Forces and",
                    new List<Word> {
                        Word1(),
                        Word2(),
                        Word3(),
                        Word4(),
                        Word5(),
                        Word6(),
                        Word7(),
                        Word8(),
                        Word9(),
                        Word10(),
                        Word11(),
                        Word12(),
                        Word13(),
                        Word14()
                    });

        public static Func<ReadResult> ReadResult1 { get; } = () => new() { Height = 11.6806, Width = 8.2639, Page = 1 };

        public static Func<OcrLineResult> OcrLineResult1 { get; } = () => new(OcrLine4(), ReadResult1(), 0, null);

        public static Func<ReconciledPiiEntity> ReconciledPiiEntity1 { get; } = () =>
            new(OcrLineResult1(), new coordinator.Services.OcrResultsService.OcrWord(Word4(), 15), "Organization", "CMS-112233");

        public static Func<ReconciledPiiEntity> ReconciledPiiEntity2 { get; } = () =>
            new(OcrLineResult1(), new coordinator.Services.OcrResultsService.OcrWord(Word12(), 61), "Organization", "CMS-112233");

        public static Func<ReconciledPiiEntity> ReconciledPiiEntity3 { get; } = () =>
            new(OcrLineResult1(), new coordinator.Services.OcrResultsService.OcrWord(Word13(), 68), "Organization", "CMS-112233");

        public static Func<IEnumerable<Line>> OcrLines { get; } = () => new[]
        {
            OcrLine1(),
            OcrLine2(),
            OcrLine3(),
            OcrLine4()
        };

        public static Func<IEnumerable<ReconciledPiiEntity>> ReconciledPiiEntities { get; } = () => new[]
        {
            ReconciledPiiEntity1(),
            ReconciledPiiEntity2(),
            ReconciledPiiEntity3()
        };
    }
}