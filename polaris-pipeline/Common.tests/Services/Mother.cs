using Common.Domain.Ocr;
using Common.Services.PiiService.Domain;
using Common.Services.PiiService.Domain.Chunking;


namespace Common.tests.Services
{
    internal static class Mother
    {
        private const string OcrLine4Text = "Joint NPCC and CPS Evidence Gathering Checklist - For Use by Police Forces and";
        private const string WhitelistedPiiTerm1 = "police";

        public static Func<Line> OcrLine1 { get; } = () => new(null, "This is line 1",
                    [
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "1", 1)
                    ]);

        public static Func<Line> OcrLine2 { get; } = () => new(null, "This is line 2",
                    [
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "2", 1)
                    ]);

        public static Func<Line> OcrLine3 { get; } = () => new(null, "This is the third line of OCR analysis",
                    [
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "the", 1),
                        new(null, "third", 1),
                        new(null, "line", 1),
                        new(null, "of", 1),
                        new(null, "OCR", 1),
                        new(null, "analysis", 1)
                    ]);


        public static Func<Word> Word1 { get; } = () => new([], "Joint", 1);
        public static Func<Word> Word2 { get; } = () => new([], "NPCC", 1);
        public static Func<Word> Word3 { get; } = () => new([], "and", 1);
        public static Func<Word> Word4 { get; } = () => new([2.3283, 1.0196, 2.6279, 1.0196, 2.6279, 1.1326, 2.3283, 1.1326], "CPS", 1);
        public static Func<Word> Word5 { get; } = () => new([], "Evidence", 1);
        public static Func<Word> Word6 { get; } = () => new([], "Gathering", 1);
        public static Func<Word> Word7 { get; } = () => new([], "Checklist", 1);
        public static Func<Word> Word8 { get; } = () => new([], "-", 1);
        public static Func<Word> Word9 { get; } = () => new([], "For", 1);
        public static Func<Word> Word10 { get; } = () => new([], "Use", 1);
        public static Func<Word> Word11 { get; } = () => new([], "by", 1);
        public static Func<Word> Word12 { get; } = () => new([5.8473, 1.0214, 6.2805, 1.0214, 6.2805, 1.1325, 5.8473, 1.1325], "Police", 1);
        public static Func<Word> Word13 { get; } = () => new([6.3399, 1.0214, 6.8222, 1.0214, 6.8222, 1.1325, 6.3399, 1.1325], "Forces", 1);
        public static Func<Word> Word14 { get; } = () => new([], "and", 1);

        public static Func<Line> OcrLine4 { get; } = () => new(null, OcrLine4Text,
                    [
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
                    ]);

        public static Func<ReadResult> ReadResult1 { get; } = () => new() { Height = 11.6806, Width = 8.2639, Page = 1 };

        public static Func<OcrLineResult> OcrLineResult1 { get; } = () => new(OcrLine4(), ReadResult1(), 0, null);

        public static Func<ReconciledPiiEntity> ReconciledPiiEntity1 { get; } = () =>
            new(OcrLineResult1(), new OcrWord(Word4(), 15), "PersonType", "Occupation", "CMS-112233", Guid.NewGuid());

        public static Func<ReconciledPiiEntity> ReconciledPiiEntity2 { get; } = () =>
            new(OcrLineResult1(), new OcrWord(Word12(), 61), "PersonType", "Occupation", "CMS-112233", Guid.NewGuid());

        public static Func<ReconciledPiiEntity> ReconciledPiiEntity3 { get; } = () =>
            new(OcrLineResult1(), new OcrWord(Word13(), 68), "PersonType", "Occupation", "CMS-112233", Guid.NewGuid());

        public static Func<PiiResultEntity> PiiResultEntity1 { get; } = () =>
            new PiiResultEntity { Text = WhitelistedPiiTerm1, Category = "Organization", ConfidenceScore = 1, Length = WhitelistedPiiTerm1.Length, Offset = OcrLine4Text.IndexOf(WhitelistedPiiTerm1, StringComparison.OrdinalIgnoreCase) };

        public static PiiResultEntityCollection PiiResultEntityCollection1 { get; set; }
        public static PiiEntitiesResult PiiEntitiesResult1 { get; set; }
        public static PiiEntitiesResultCollection PiiEntitiesResultCollection1 { get; set; }
        public static PiiEntitiesWrapper PiiEntitiesWrapper1 { get; set; }

        public static Func<IEnumerable<Line>> OcrLines { get; } = () =>
        [
            OcrLine1(),
            OcrLine2(),
            OcrLine3(),
            OcrLine4()
        ];

        public static Func<IEnumerable<ReconciledPiiEntity>> ReconciledPiiEntities { get; } = () =>
        [
            ReconciledPiiEntity1(),
            ReconciledPiiEntity2(),
            ReconciledPiiEntity3()
        ];

        public static Func<IEnumerable<PiiResultEntity>> PiiResultEntities { get; } = () =>
        [
            PiiResultEntity1(),
        ];

        public static void BuildModels()
        {
            PiiResultEntityCollection1 = [.. PiiResultEntities()];

            PiiEntitiesResult1 = new PiiEntitiesResult { Entities = PiiResultEntityCollection1, Id = "7b14998e-cba6-4204-8161-9202a12b5c85" };

            PiiEntitiesResultCollection1 = new PiiEntitiesResultCollection
            {
                Items = []
            };
            PiiEntitiesResultCollection1.Items.Add(PiiEntitiesResult1);

            PiiEntitiesWrapper1 = new PiiEntitiesWrapper
            {
                PiiResultCollection = [PiiEntitiesResultCollection1]
            };
        }
    }
}