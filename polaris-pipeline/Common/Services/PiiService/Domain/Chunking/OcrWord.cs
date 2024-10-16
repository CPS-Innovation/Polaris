using Common.Domain.Ocr;

namespace Common.Services.PiiService.Domain.Chunking
{
    public class OcrWord : Word
    {
        public OcrWord(Word word, int accumulativeOffset) : base(word.BoundingBox, word.Text, word.Confidence)
        {
            SetOffsetRange(accumulativeOffset);
        }

        public int TextLength => Text.Length;
        public (int Min, int Max) RelativeOffset { get; protected set; }

        private void SetOffsetRange(int accumulativeOffset)
        {
            RelativeOffset = (accumulativeOffset, accumulativeOffset + TextLength - 1);
        }
    }
}