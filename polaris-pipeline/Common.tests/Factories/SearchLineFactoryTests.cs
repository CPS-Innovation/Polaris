using System.Text;
using AutoFixture;
using Common.Factories;
using Common.Factories.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit;

namespace Common.tests.Factories
{
	public class SearchLineFactoryTests
	{
		private readonly Guid _polarisDocumentId;
        private readonly long _caseId;
		private readonly string _documentId;
		private readonly long _versionId;
		private readonly string _blobName;
		private readonly ReadResult _readResult;
		private readonly Line _line;
		private readonly int _index;
        private readonly double _pageHeight;
        private readonly double _pageWidth;

		private readonly ISearchLineFactory _searchLineFactory;

		public SearchLineFactoryTests()
        {
            var fixture = new Fixture();
            _polarisDocumentId = fixture.Create<Guid>();
            _caseId = fixture.Create<int>();
			_documentId = fixture.Create<string>();
			_versionId = fixture.Create<long>();
			_blobName = fixture.Create<string>();
			fixture.Create<string>();
            _pageHeight = fixture.Create<double>();
			_pageWidth = fixture.Create<double>();
			_readResult = new ReadResult
			{
				Page = fixture.Create<int>(),
				Width = _pageWidth,
				Height = _pageHeight
            };
			_line = fixture.Create<Line>();
			_index = fixture.Create<int>();

			_searchLineFactory = new SearchLineFactory();
		}

		[Fact]
		public void Create_ReturnsExpectedId()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			var id = $"{_polarisDocumentId}-{_readResult.Page}-{_index}";
			var bytes = Encoding.UTF8.GetBytes(id);
			var base64Id = Convert.ToBase64String(bytes);

			factory.Id.Should().Be(base64Id);
		}
		
		[Fact]
		public void Create_ReturnsExpectedBlobName()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.FileName.Should().Be(_blobName);
		}
		
		[Fact]
		public void Create_ReturnsExpectedPageIndex()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.PageIndex.Should().Be(_readResult.Page);
		}

		[Fact]
		public void Create_ReturnsExpectedLineIndex()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.LineIndex.Should().Be(_index);
		}

		[Fact]
		public void Create_ReturnsExpectedLanguage()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.Language.Should().Be(_line.Language);
		}

		[Fact]
		public void Create_ReturnsExpectedBoundingBox()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.BoundingBox.Should().BeEquivalentTo(_line.BoundingBox);
		}

		[Fact]
		public void Create_ReturnsExpectedAppearance()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.Appearance.Should().Be(_line.Appearance);
		}

		[Fact]
		public void Create_ReturnsExpectedText()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.Text.Should().Be(_line.Text);
		}

		[Fact]
		public void Create_ReturnsExpectedWords()
		{
			var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

			factory.Words.Should().BeEquivalentTo(_line.Words);
		}

        [Fact]
        public void Create_ReturnsExpectedHeightAndWidth()
        {
            var factory = _searchLineFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _readResult, _line, _index);

            using (new AssertionScope())
            {
                factory.PageHeight.Should().Be(_pageHeight);
                factory.PageWidth.Should().Be(_pageWidth);
            }
		}
	}
}

