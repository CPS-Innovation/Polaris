// <copyright file="CaseMaterialServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cps.Fct.Hk.Ui.Services.Tests
{
    public class CaseMaterialServiceTests
    {
        private readonly TestLogger<CaseMaterialService> mockLogger;
        private readonly Mock<ICommunicationService> mockCommunicationService;
        private readonly Mock<IDocumentTypeMapper> mockDocumentTypeMapper;
        private readonly CaseMaterialService caseMaterialService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseMaterialServiceTests"/> class.
        /// Sets up mock dependencies and initializes the <see cref="CaseMaterialService"/> to be tested.
        /// </summary>
        public CaseMaterialServiceTests()
        {
            this.mockLogger = new TestLogger<CaseMaterialService>();
            this.mockCommunicationService = new Mock<ICommunicationService>();
            this.mockDocumentTypeMapper = new Mock<IDocumentTypeMapper>();

            this.caseMaterialService = new CaseMaterialService(
                this.mockLogger,
                this.mockCommunicationService.Object,
                this.mockDocumentTypeMapper.Object);
        }

        /// <summary>
        /// Tests the MapCommunicationsToCaseMaterials method to ensure it correctly maps a list of Communication objects
        /// into CaseMaterial objects.
        /// </summary>
        [Fact]
        public void MapCommunicationsToCaseMaterials_ReturnsMappedCaseMaterials()
        {
            // Arrange
            var communications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Category1", "TypeA", false, Direction: "Incoming"),
            new Communication(1, "FileB.pdf", "Subject A", 1012, 456, "/some/path/doc2.pdf", "None", "Category2", "TypeB", false, Direction: "Outgoing"),
        };

            this.mockDocumentTypeMapper
             .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
             .Returns(new List<DocumentTypeGroup> { new() { Id = 1012, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapCommunicationsToCaseMaterials(communications);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("FileA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("Subject A", firstMaterial.Subject);
            Assert.Equal(1012, firstMaterial.DocumentTypeId);
            Assert.Equal(123, firstMaterial.MaterialId);
            Assert.Equal("/some/path/doc1.pdf", firstMaterial.Link);
            Assert.Equal("Category1", firstMaterial.Category);
            Assert.Equal("TypeA", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("None", firstMaterial.Status);
            Assert.True(firstMaterial.IsReclassifiable);
            Assert.False(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapCommunicationsToCaseMaterials method with an empty list input.
        /// Ensures that it returns an empty list instead of null.
        /// </summary>
        [Fact]
        public void MapCommunicationsToCaseMaterials_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var communications = new List<Communication>();

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapCommunicationsToCaseMaterials(communications);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapCommunicationsToCaseMaterials method with a null input.
        /// Ensures that it correctly handles null values by returning an empty list.
        /// </summary>
        [Fact]
        public void MapCommunicationsToCaseMaterials_WithNullInput_ReturnsEmptyList()
        {
            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapCommunicationsToCaseMaterials(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedExhibitsToCaseMaterials method to ensure it correctly maps a list of Exhibit objects
        /// into CaseMaterial objects with the appropriate properties.
        /// </summary>
        [Fact]
        public void MapUsedExhibitsToCaseMaterials_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);

            int caseId = 123;
            var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

            var usedExhibitsResponse = new UsedExhibitsResponse
            {
                Exhibits = new List<Exhibit>
            {
                new Exhibit(1, "ExhibitA.pdf", "OriginalA.pdf", "1202", 1202, "/some/path/exhibit1.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
                new Exhibit(2, "ExhibitB.pdf", "OriginalB.pdf", "1202", 1202, "/some/path/exhibit2.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
            },
            };

            ExhibitProducersResponse exhibitProducersResponse = BuildTestExhibitProducers();

            var communications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 1, "/some/path/doc1.pdf", "None", "Category1", "TypeA", false, Direction: "Incoming"),
            new Communication(1, "FileB.pdf", "Subject A", 1012, 2, "/some/path/doc2.pdf", "None", "Category2", "TypeB", false, Direction: "Outgoing"),
        };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1202))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1202, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedExhibitsToCaseMaterials(usedExhibitsResponse, exhibitProducersResponse, communications, caseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("Subject A", firstMaterial.Subject);
            Assert.Equal(1202, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/exhibit1.pdf", firstMaterial.Link);
            Assert.Equal("Mapped Category A", firstMaterial.Category);
            Assert.Equal("Mapped DocumentType A", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.Equal("some-reference", firstMaterial.Reference);
            Assert.Equal("some-producer", firstMaterial.Producer);
            Assert.True(firstMaterial.IsReclassifiable);
            Assert.True(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedExhibitsToCaseMaterials method to ensure it correctly maps a list of Exhibit objects
        /// into CaseMaterial objects with the appropriate properties when it has a null DocumentType.
        /// </summary>
        [Fact]
        public void MapUsedExhibitsToCaseMaterials_WithNullDocumentType_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            int caseId = 123;
            var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

            var usedExhibitsResponse = new UsedExhibitsResponse
            {
                Exhibits = new List<Exhibit>
            {
                new Exhibit(1, "ExhibitA.pdf", "OriginalA.pdf", "1202", null, "/some/path/exhibit1.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
                new Exhibit(2, "ExhibitB.pdf", "OriginalB.pdf", "1202", 1202, "/some/path/exhibit2.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
            },
            };

            var exhibitProducersResponse = new ExhibitProducersResponse()
            {
                ExhibitProducers = new List<ExhibitProducer>()
            {
                new (1212, "some producer 1", false),
                new (1213, "some producer 2", false),
                new (1214, "some producer 3", false),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(0))
                .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1202))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1202, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedExhibitsToCaseMaterials(usedExhibitsResponse, exhibitProducersResponse, null, caseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapDocumentType(0), Times.Once());

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("ExhibitA.pdf", firstMaterial.Subject);
            Assert.Equal(0, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/exhibit1.pdf", firstMaterial.Link);
            Assert.Equal("Unknown", firstMaterial.Category);
            Assert.Equal("Unknown", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.False(firstMaterial.IsReclassifiable);
            Assert.True(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedExhibitsToCaseMaterials method with an empty list of exhibits.
        /// Ensures that it correctly returns an empty list instead of null.
        /// </summary>
        [Fact]
        public void MapUsedExhibitsToCaseMaterials_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var usedExhibitsResponse = new UsedExhibitsResponse
            {
                Exhibits = new List<Exhibit>(),
            };

            ExhibitProducersResponse exhibitProducersResponse = BuildTestExhibitProducers();

            int caseId = 123;

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedExhibitsToCaseMaterials(usedExhibitsResponse, exhibitProducersResponse, null, caseId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedExhibitsToCaseMaterials method with a null input for UsedExhibitsResponse.
        /// Ensures that it correctly handles null values by returning an empty list.
        /// </summary>
        [Fact]
        public void MapUsedExhibitsToCaseMaterials_WithNullInput_ReturnsEmptyList()
        {
            // Act
            int caseId = 123;
            List<CaseMaterial> result = this.caseMaterialService.MapUsedExhibitsToCaseMaterials(null, null, null, caseId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedExhibitsToCaseMaterials method when the UsedExhibitsResponse is provided,
        /// but the Exhibits property itself is null. Ensures that it returns an empty list instead of throwing an exception.
        /// </summary>
        [Fact]
        public void MapUsedExhibitsToCaseMaterials_WithNullExhibits_ReturnsEmptyList()
        {
            // Arrange
            int caseId = 123;
            var usedExhibitsResponse = new UsedExhibitsResponse
            {
                Exhibits = null,
            };

            ExhibitProducersResponse exhibitProducersResponse = BuildTestExhibitProducers();

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedExhibitsToCaseMaterials(usedExhibitsResponse, exhibitProducersResponse, null, caseId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method to ensure it correctly maps a list of Statement objects
        /// into CaseMaterial objects with the appropriate properties.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var usedStatementsResponse = new UsedStatementsResponse
            {
                Statements = new List<Statement>
            {
                new Statement(1, 789, "StatementA.pdf", "OriginalA.pdf", "1202", 1202, "/some/path/statement1.pdf", "Pending", receivedDate, statementTakenDate),
                new Statement(2, 789, "StatementB.pdf", "OriginalB.pdf", "1202", 1202, "/some/path/statement2.pdf", "Pending", receivedDate, statementTakenDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1202))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1202, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatementsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("OriginalA", firstMaterial.Subject);
            Assert.Equal(1202, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/statement1.pdf", firstMaterial.Link);
            Assert.Equal("Mapped Category A", firstMaterial.Category);
            Assert.Equal("Mapped DocumentType A", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.Equal(statementTakenDate, firstMaterial.RecordedDate);
            Assert.True(firstMaterial.IsReclassifiable);
            Assert.True(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method to ensure it uses Title as fallback for Subject
        /// when OriginalFileName is null or blank.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_UsesToTitleAsFallbackWhenOriginalFileNameIsNullOrBlank()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var usedStatementsResponse = new UsedStatementsResponse
            {
                Statements = new List<Statement>
            {
                // Statement with null OriginalFileName
                new Statement(1, 789, "1", null!, "1202", 1202, "/some/path/statement1.pdf", "Pending", receivedDate, statementTakenDate),

                // Statement with empty OriginalFileName
                new Statement(2, 789, "2", string.Empty, "1202", 1202, "/some/path/statement2.pdf", "Pending", receivedDate, statementTakenDate),

                // Statement with whitespace-only OriginalFileName
                new Statement(3, 789, "3", "   ", "1202", 1202, "/some/path/statement3.pdf", "Pending", receivedDate, statementTakenDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1202))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1204, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatementsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            // Test null OriginalFileName case
            CaseMaterial firstMaterial = result[0];
            Assert.Equal(1, firstMaterial.Id);
            Assert.Null(firstMaterial.OriginalFileName);
            Assert.Equal("1", firstMaterial.Subject); // Should use Title, not PresentationTitle
            Assert.Equal(1202, firstMaterial.DocumentTypeId);

            // Test empty OriginalFileName case
            CaseMaterial secondMaterial = result[1];
            Assert.Equal(2, secondMaterial.Id);
            Assert.Equal(string.Empty, secondMaterial.OriginalFileName);
            Assert.Equal("2", secondMaterial.Subject); // Should use Title, not PresentationTitle

            // Test whitespace OriginalFileName case
            CaseMaterial thirdMaterial = result[2];
            Assert.Equal(3, thirdMaterial.Id);
            Assert.Equal("   ", thirdMaterial.OriginalFileName);
            Assert.Equal("3", thirdMaterial.Subject); // Should use Title, not PresentationTitle
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method to ensure it correctly maps a list of Statement objects
        /// into CaseMaterial objects with the appropriate properties when the Witness Id is null.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_WithNullWitnessId_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var usedStatementsResponse = new UsedStatementsResponse
            {
                Statements = new List<Statement>
            {
                new Statement(1, null, "StatementA.pdf", "OriginalA.pdf", "1202", 1202, "/some/path/statement1.pdf", "Pending", receivedDate, statementTakenDate),
                new Statement(2, null, "StatementB.pdf", "OriginalB.pdf", "1202", 1202, "/some/path/statement2.pdf", "Pending", receivedDate, statementTakenDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1202))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1202, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatementsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("OriginalA", firstMaterial.Subject);
            Assert.Equal(1202, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/statement1.pdf", firstMaterial.Link);
            Assert.Equal("Mapped Category A", firstMaterial.Category);
            Assert.Equal("Mapped DocumentType A", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.Equal(statementTakenDate, firstMaterial.RecordedDate);
            Assert.True(firstMaterial.IsReclassifiable);
            Assert.True(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method to ensure it correctly maps a list of Statement objects
        /// into CaseMaterial objects with the appropriate properties when it has a null DocumentType.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_WithNullDocumentType_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var usedStatementsResponse = new UsedStatementsResponse
            {
                Statements = new List<Statement>
            {
                new Statement(1, 789, "StatementA.pdf", "OriginalA.pdf", "1202", null, "/some/path/statement1.pdf", "Pending", receivedDate, statementTakenDate),
                new Statement(2, 789, "StatementB.pdf", "OriginalB.pdf", "1202", 1202, "/some/path/statement2.pdf", "Pending", receivedDate, statementTakenDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(0))
                .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1202))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
               .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
               .Returns(new List<DocumentTypeGroup> { new() { Id = 1204, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatementsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapDocumentType(0), Times.Once());

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("OriginalA", firstMaterial.Subject);
            Assert.Equal(0, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/statement1.pdf", firstMaterial.Link);
            Assert.Equal("Unknown", firstMaterial.Category);
            Assert.Equal("Unknown", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.Equal(statementTakenDate, firstMaterial.RecordedDate);
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method with an empty list of statements.
        /// Ensures that it correctly returns an empty list instead of null.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var usedStatementsResponse = new UsedStatementsResponse
            {
                Statements = new List<Statement>(),
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatementsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method with a null input for UsedStatementsResponse.
        /// Ensures that it correctly handles null values by returning an empty list.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_WithNullInput_ReturnsEmptyList()
        {
            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedStatementsToCaseMaterials method when the UsedStatementsResponse is provided,
        /// but the Statements property itself is null. Ensures that it returns an empty list instead of throwing an exception.
        /// </summary>
        [Fact]
        public void MapUsedStatementsToCaseMaterials_WithNullStatements_ReturnsEmptyList()
        {
            // Arrange
            var usedStatementsResponse = new UsedStatementsResponse
            {
                Statements = null,
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatementsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUnusedMaterialsToCaseMaterials method to ensure it correctly maps
        /// a list of unused materials (Exhibits, MgForms, OtherMaterials, and Statements)
        /// into CaseMaterial objects with the appropriate properties.
        /// </summary>
        [Fact]
        public void MapUnusedMaterialsToCaseMaterials_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var unusedMaterialsResponse = new UnusedMaterialsResponse
            {
                Exhibits = new List<Exhibit>
            {
                new Exhibit(1, "ExhibitA.pdf", "OriginalA.pdf", "1202", 1001, "/some/path/exhibit1.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
                new Exhibit(2, "ExhibitB.pdf", "OriginalB.pdf", "1202", 1002, "/some/path/exhibit2.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
            },
                MgForms = new List<MgForm>
            {
                new MgForm(3, "MgFormA.pdf", "OriginalC.pdf", "1202", 1003, "/some/path/mgform1.pdf", "Pending", receivedDate),
            },
                OtherMaterials = new List<MgForm>
            {
                new MgForm(4, "OtherMaterialA.pdf", "OriginalD.pdf", "1202", 1004, "/some/path/other1.pdf", "Pending", receivedDate),
            },
                Statements = new List<Statement>
            {
                new Statement(5, 789, "StatementA.pdf", "OriginalE.pdf", "1202", 1005, "/some/path/statement1.pdf", "Pending", receivedDate, statementTakenDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1202"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapDocumentType(1005))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
          .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
          .Returns(new List<DocumentTypeGroup> { new() { Id = 1001, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(unusedMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count); // Total count from all categories

            // Validate first item
            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("ExhibitA.pdf", firstMaterial.Subject);
            Assert.Equal(1001, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/exhibit1.pdf", firstMaterial.Link);
            Assert.Equal("Mapped Category A", firstMaterial.Category);
            Assert.Equal("Mapped DocumentType A", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Unused", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.Equal(statementTakenDate, result[4].RecordedDate); // statement case material.
            Assert.True(firstMaterial.IsReclassifiable);

            // Validate statement
            CaseMaterial statementMaterial = result.Last();
            Assert.Equal(5, statementMaterial.Id);
            Assert.Equal("OriginalE.pdf", statementMaterial.OriginalFileName);
            Assert.Equal("OriginalE", statementMaterial.Subject);
            Assert.Equal(1005, statementMaterial.DocumentTypeId);
        }

        /// <summary>
        /// Tests the MapUnusedMaterialsToCaseMaterials method to ensure it correctly maps
        /// a list of unused materials (Exhibits, MgForms, OtherMaterials, and Statements) into
        /// CaseMaterial objects with the appropriate properties when it has a null MaterialType.
        /// </summary>
        [Fact]
        public void MapUnusedMaterialsToCaseMaterials_WithNullDocumentType_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var unusedMaterialsResponse = new UnusedMaterialsResponse
            {
                Exhibits = new List<Exhibit>
            {
                new Exhibit(1, "ExhibitA.pdf", "OriginalA.pdf", null, null, "/some/path/exhibit1.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
                new Exhibit(2, "ExhibitB.pdf", "OriginalB.pdf", "1202", 1002, "/some/path/exhibit2.pdf", "Pending", receivedDate, "some-reference", "some-producer"),
            },
                MgForms = new List<MgForm>
            {
                new MgForm(3, "MgFormA.pdf", "OriginalC.pdf", null, null, "/some/path/mgform1.pdf", "Pending", receivedDate),
            },
                OtherMaterials = new List<MgForm>
            {
                new MgForm(4, "OtherMaterialA.pdf", "OriginalD.pdf", null, null, "/some/path/other1.pdf", "Pending", receivedDate),
            },
                Statements = new List<Statement>
            {
                new Statement(5, 789, "StatementA.pdf", "OriginalE.pdf", null, null, "/some/path/statement1.pdf", "Pending", receivedDate, statementTakenDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("0"))
                .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1202"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
              .Setup(mapper => mapper.MapDocumentType(0))
             .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
              .Setup(mapper => mapper.MapDocumentType(1202))
              .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
               .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
               .Returns(new List<DocumentTypeGroup> { new() { Id = 1204, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(unusedMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count); // Total count from all categories

            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapMaterialType("0"), Times.Exactly(3));
            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapDocumentType(0), Times.Once());

            // Validate first item
            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("ExhibitA.pdf", firstMaterial.Subject);
            Assert.Equal(0, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/exhibit1.pdf", firstMaterial.Link);
            Assert.Equal("Unknown", firstMaterial.Category);
            Assert.Equal("Unknown", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Unused", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.Equal(statementTakenDate, result[4].RecordedDate); // statement case material.
        }

        /// <summary>
        /// Tests the MapUnusedMaterialsToCaseMaterials method with empty lists for all categories.
        /// Ensures that it correctly returns an empty list instead of null.
        /// </summary>
        [Fact]
        public void MapUnusedMaterialsToCaseMaterials_WithEmptyLists_ReturnsEmptyList()
        {
            // Arrange
            var unusedMaterialsResponse = new UnusedMaterialsResponse
            {
                Exhibits = new List<Exhibit>(),
                MgForms = new List<MgForm>(),
                OtherMaterials = new List<MgForm>(),
                Statements = new List<Statement>(),
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(unusedMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUnusedMaterialsToCaseMaterials method with a null input.
        /// Ensures that it correctly handles null values by returning an empty list.
        /// </summary>
        [Fact]
        public void MapUnusedMaterialsToCaseMaterials_WithNullInput_ReturnsEmptyList()
        {
            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUnusedMaterialsToCaseMaterials method when the UnusedMaterialsResponse is provided,
        /// but all its material properties (Exhibits, MgForms, OtherMaterials, and Statements) are null.
        /// Ensures that it returns an empty list instead of throwing an exception.
        /// </summary>
        [Fact]
        public void MapUnusedMaterialsToCaseMaterials_WithNullProperties_ReturnsEmptyList()
        {
            // Arrange
            var unusedMaterialsResponse = new UnusedMaterialsResponse
            {
                Exhibits = null,
                MgForms = null,
                OtherMaterials = null,
                Statements = null,
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(unusedMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetCommunications.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetCommunications()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            this.mockCommunicationService
                .Setup(x => x.GetCommunicationsAsync(123, It.IsAny<CmsAuthValues>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetCommunicationsAsync(123, cmsAuthValues));

            Assert.Equal("An error was encountered fetching communications for caseId [123]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching communications for caseId [123]: Unexpected error"));
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetUnusedMaterials.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetUnusedMaterials()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            this.mockCommunicationService
                .Setup(x => x.GetUnusedMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetUnusedMaterialsAsync(123, cmsAuthValues));

            Assert.Equal("An error was encountered fetching unused materials for caseId [123]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving unused materials for caseId"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching unused materials for caseId [123]: Unexpected error"));
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetUsedStatements.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetUsedStatements()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            this.mockCommunicationService
                .Setup(x => x.GetUsedStatementsAsync(123, It.IsAny<CmsAuthValues>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetUsedStatementsAsync(123, cmsAuthValues));

            Assert.Equal("An error was encountered fetching used statements for caseId [123]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving used statements for caseId"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used statements for caseId [123]: Unexpected error"));
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetUsedExhibits.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetUsedExhibits()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            this.mockCommunicationService
                .Setup(x => x.GetUsedExhibitsAsync(123, It.IsAny<CmsAuthValues>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetUsedExhibitsAsync(123, cmsAuthValues));

            Assert.Equal("An error was encountered fetching used exhibits for caseId [123]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving used exhibits for caseId"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used exhibits for caseId [123]: Unexpected error"));
        }

        /// <summary>
        /// Tests the MapUsedMgFormsToCaseMaterials method to ensure it correctly maps a list of MgForm objects
        /// into CaseMaterial objects with the appropriate properties.
        /// </summary>
        [Fact]
        public void MapUsedMgFormsToCaseMaterials_ReturnsMappedCaseMaterials()
        {
            // Arrange
            var receivedDate = new DateTime(2025, 04, 01);
            var usedMgFormsResponse = new UsedMgFormsResponse
            {
                MgForms = new List<MgForm>
            {
                new MgForm(1, "MgFormA.pdf", "OriginalA.pdf", "1202", 1202, "/some/path/mgform1.pdf", "Pending", receivedDate),
                new MgForm(2, "MgFormB.pdf", "OriginalB.pdf", "1203", 1203, "/some/path/mgform2.pdf", "Complete", receivedDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1202"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1203"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category B", DocumentType = "Mapped DocumentType B" });

            this.mockDocumentTypeMapper
               .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
               .Returns(new List<DocumentTypeGroup> { new() { Id = 1202, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedMgFormsToCaseMaterials(usedMgFormsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("MgFormA.pdf", firstMaterial.Subject);
            Assert.Equal(1202, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/mgform1.pdf", firstMaterial.Link);
            Assert.Equal("Mapped Category A", firstMaterial.Category);
            Assert.Equal("Mapped DocumentType A", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);

            CaseMaterial secondMaterial = result.Last();
            Assert.Equal("Mapped Category B", secondMaterial.Category);
            Assert.Equal("Mapped DocumentType B", secondMaterial.Type);
            Assert.True(firstMaterial.IsReclassifiable);
            Assert.False(secondMaterial.IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedMgFormsToCaseMaterials method to ensure it correctly maps a list of MgForm objects
        /// into CaseMaterial objects with the appropriate properties when it has a null DocumentType.
        /// </summary>
        [Fact]
        public void MapUsedMgFormsToCaseMaterials_WithNullDocumentType_ReturnsMappedCaseMaterials()
        {
            // Arrange
            var receivedDate = new DateTime(2025, 04, 01);
            var usedMgFormsResponse = new UsedMgFormsResponse
            {
                MgForms = new List<MgForm>
            {
                new MgForm(1, "MgFormA.pdf", "OriginalA.pdf", null, null, "/some/path/mgform1.pdf", "Pending", receivedDate),
                new MgForm(2, "MgFormB.pdf", "OriginalB.pdf", "1202", 1202, "/some/path/mgform2.pdf", "Complete", receivedDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("0"))
                .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1202"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
               .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
               .Returns(new List<DocumentTypeGroup> { new() { Id = 1202, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedMgFormsToCaseMaterials(usedMgFormsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapMaterialType("0"), Times.Once());

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("MgFormA.pdf", firstMaterial.Subject);
            Assert.Equal(0, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/mgform1.pdf", firstMaterial.Link);
            Assert.Equal("Unknown", firstMaterial.Category);
            Assert.Equal("Unknown", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.False(firstMaterial.IsReclassifiable);
            Assert.True(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedMgFormsToCaseMaterials method with an empty list of MG forms.
        /// Ensures that it correctly returns an empty list instead of null.
        /// </summary>
        [Fact]
        public void MapUsedMgFormsToCaseMaterials_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var usedMgFormsResponse = new UsedMgFormsResponse
            {
                MgForms = new List<MgForm>(),
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedMgFormsToCaseMaterials(usedMgFormsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedMgFormsToCaseMaterials method with a null input for UsedMgFormsResponse.
        /// Ensures that it correctly handles null values by returning an empty list.
        /// </summary>
        [Fact]
        public void MapUsedMgFormsToCaseMaterials_WithNullInput_ReturnsEmptyList()
        {
            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedMgFormsToCaseMaterials(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedMgFormsToCaseMaterials method when the UsedMgFormsResponse is provided,
        /// but the MgForms property itself is null. Ensures that it returns an empty list instead of throwing an exception.
        /// </summary>
        [Fact]
        public void MapUsedMgFormsToCaseMaterials_WithNullMgForms_ReturnsEmptyList()
        {
            // Arrange
            var usedMgFormsResponse = new UsedMgFormsResponse
            {
                MgForms = null,
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedMgFormsToCaseMaterials(usedMgFormsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedOtherMaterialsToCaseMaterials method to ensure it correctly maps a list of MgForm objects
        /// into CaseMaterial objects with the appropriate properties.
        /// </summary>
        [Fact]
        public void MapUsedOtherMaterialsToCaseMaterials_ReturnsMappedCaseMaterials()
        {
            // Arrange
            var receivedDate = new DateTime(2025, 04, 01);
            var usedOtherMaterialsResponse = new UsedOtherMaterialsResponse
            {
                MgForms = new List<MgForm>
            {
                new MgForm(1, "OtherMaterialA.pdf", "OriginalA.pdf", "1204", 1204, "/some/path/other1.pdf", "Pending", receivedDate),
                new MgForm(2, "OtherMaterialB.pdf", "OriginalB.pdf", "1205", 1205, "/some/path/other2.pdf", "Complete", receivedDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1204"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1205"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category B", DocumentType = "Mapped DocumentType B" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1204, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("OtherMaterialA.pdf", firstMaterial.Subject);
            Assert.Equal(1204, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/other1.pdf", firstMaterial.Link);
            Assert.Equal("Mapped Category A", firstMaterial.Category);
            Assert.Equal("Mapped DocumentType A", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.True(firstMaterial.IsReclassifiable);
            Assert.False(result.Last().IsReclassifiable);

            CaseMaterial secondMaterial = result.Last();
            Assert.Equal("Mapped Category B", secondMaterial.Category);
            Assert.Equal("Mapped DocumentType B", secondMaterial.Type);
        }

        /// <summary>
        /// Tests the MapUsedOtherMaterialsToCaseMaterials method to ensure it correctly maps a list of MgForm objects
        /// into CaseMaterial objects with the appropriate properties when it has a null DocumentType.
        /// </summary>
        [Fact]
        public void MapUsedOtherMaterialsToCaseMaterials_WithNullDocumentType_ReturnsMappedCaseMaterials()
        {
            // Arrange
            var receivedDate = new DateTime(2025, 04, 01);
            var usedOtherMaterialsResponse = new UsedOtherMaterialsResponse
            {
                MgForms = new List<MgForm>
            {
                new MgForm(1, "OtherMaterialA.pdf", "OriginalA.pdf", null, null, "/some/path/other1.pdf", "Pending", receivedDate),
                new MgForm(2, "OtherMaterialB.pdf", "OriginalB.pdf", "1204", 1204, "/some/path/other2.pdf", "Complete", receivedDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("0"))
                .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1204"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1204, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapMaterialType("0"), Times.Once());

            CaseMaterial firstMaterial = result.First();
            Assert.Equal(1, firstMaterial.Id);
            Assert.Equal("OriginalA.pdf", firstMaterial.OriginalFileName);
            Assert.Equal("OtherMaterialA.pdf", firstMaterial.Subject);
            Assert.Equal(0, firstMaterial.DocumentTypeId);
            Assert.Equal(1, firstMaterial.MaterialId);
            Assert.Equal("/some/path/other1.pdf", firstMaterial.Link);
            Assert.Equal("Unknown", firstMaterial.Category);
            Assert.Equal("Unknown", firstMaterial.Type);
            Assert.False(firstMaterial.HasAttachments);
            Assert.Equal("Used", firstMaterial.Status);
            Assert.Equal(receivedDate, firstMaterial.Date);
            Assert.False(firstMaterial.IsReclassifiable);
            Assert.True(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Tests the MapUsedOtherMaterialsToCaseMaterials method with an empty list of other materials.
        /// Ensures that it correctly returns an empty list instead of null.
        /// </summary>
        [Fact]
        public void MapUsedOtherMaterialsToCaseMaterials_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var usedOtherMaterialsResponse = new UsedOtherMaterialsResponse
            {
                MgForms = new List<MgForm>(),
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedOtherMaterialsToCaseMaterials method with a null input for UsedOtherMaterialsResponse.
        /// Ensures that it correctly handles null values by returning an empty list.
        /// </summary>
        [Fact]
        public void MapUsedOtherMaterialsToCaseMaterials_WithNullInput_ReturnsEmptyList()
        {
            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedOtherMaterialsToCaseMaterials(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the MapUsedOtherMaterialsToCaseMaterials method when the UsedOtherMaterialsResponse is provided,
        /// but the MgForms property itself is null. Ensures that it returns an empty list instead of throwing an exception.
        /// </summary>
        [Fact]
        public void MapUsedOtherMaterialsToCaseMaterials_WithNullMgForms_ReturnsEmptyList()
        {
            // Arrange
            var usedOtherMaterialsResponse = new UsedOtherMaterialsResponse
            {
                MgForms = null,
            };

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetUsedMgForms.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetUsedMgForms()
        {
            // Arrange
            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            this.mockCommunicationService
                .Setup(x => x.GetUsedMgFormsAsync(123, It.IsAny<CmsAuthValues>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetUsedMgFormsAsync(123, cmsAuthValues));

            Assert.Equal("An error was encountered fetching used MG forms for caseId [123]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving used MG forms for caseId"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used MG forms for caseId [123]: Unexpected error"));
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetUsedOtherMaterials.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetUsedOtherMaterials()
        {
            // Arrange
            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            this.mockCommunicationService
                .Setup(x => x.GetUsedOtherMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetUsedOtherMaterialsAsync(123, cmsAuthValues));

            Assert.Equal("An error was encountered fetching used other materials for caseId [123]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving used other materials for caseId"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used other materials for caseId [123]: Unexpected error"));
        }

        /// <summary>
        /// Tests that the service returns an unprocessable error when an exception is thrown by GetExhibitProducersAsync.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetExhibitProducersAsync()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            ExhibitProducersResponse exhibitProducersResponse = BuildTestExhibitProducers();

            this.mockCommunicationService
                .Setup(x => x.GetExhibitProducersAsync(234, cmsAuthValues))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            UnprocessableEntityException exception = await Assert.ThrowsAsync<UnprocessableEntityException>(
                () => this.caseMaterialService.GetExhibitProducersAsync(234, cmsAuthValues));

            Assert.Equal("An error was encountered fetching exhibit producers for caseId [234]", exception.Message);

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving exhibit producers for caseId [234]"));

            Assert.Contains(this.mockLogger.Logs, log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching exhibit producers for caseId [234]: Unexpected error"));
        }

        /// <summary>
        /// Tests that the service returns null when exhibit producer is not provided.
        /// </summary>
        [Fact]
        public void MapExistingProducerOrWitnessId_ReturnsNull_WhenExhibitProducerIsNotProvided()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            ExhibitProducersResponse exhibitProducersResponse = BuildTestExhibitProducers();

            this.mockCommunicationService
                .Setup(x => x.GetExhibitProducersAsync(234, cmsAuthValues))
                .ReturnsAsync(new ExhibitProducersResponse());

            // Act
            var result = this.caseMaterialService.MapExistingProducerOrWitnessId(string.Empty, exhibitProducersResponse, 321);

            // Assert
            Assert.Null(result);

            this.mockCommunicationService.Verify(x => x.GetExhibitProducersAsync(234, cmsAuthValues), Times.Never());
        }

        /// <summary>
        /// Tests that the service returns existing producer or witness Id when exhibit producer provided is matched from the query.
        /// </summary>
        /// <param name="producer">The exhibit producer name passed from used exhibit.</param>
        [InlineData("some producer 2")]
        [InlineData("Some producER 2")]
        [InlineData("somE PRODUCER 2")]
        [InlineData("SOME PRODUCER 2")]
        [Theory]
        public void MapExistingProducerOrWitnessId_ReturnsId_WhenExhibitProducerIsMatchedWithQueryResult(string producer)
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var cmsAuthValues = new CmsAuthValues(
                "cookies",
                "token",
                Guid.NewGuid());

            var exhibitProducers = new ExhibitProducersResponse()
            {
                ExhibitProducers =
                [
                    new (1212, "some producer 1", false),
                new (1213, producer, false),
                new (1214, "some producer 3", false),
            ],
            };

            this.mockCommunicationService
                .Setup(x => x.GetExhibitProducersAsync(234, cmsAuthValues))
                .ReturnsAsync(exhibitProducers);

            // Act
            int? result = this.caseMaterialService.MapExistingProducerOrWitnessId(producer, exhibitProducers, 234);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(1213, result.Value);
        }

        /// <summary>
        /// Tests the MapUnusedMaterialsToCaseMaterials method to ensure it correctly maps
        /// Unused MgForms and OtherMaterials with CaseMaterial objects with the appropriate properties when it has a null document type but provided material type.
        /// </summary>
        [Fact]
        public void MapUnusedMgFormsAndOtherMaterialsToCaseMaterials_WithNullDocumentType_And_ProvidedMaterialType_ReturnsMappedCaseMaterials()
        {
            // Arrange
            DateTime receivedDate = new DateTime(2025, 04, 01);
            DateTime statementTakenDate = new DateTime(2025, 03, 02);
            var unusedMaterialsResponse = new UnusedMaterialsResponse
            {
                MgForms = new List<MgForm>
            {
                new MgForm(1, "MgFormA.pdf", "OriginalC.pdf", "1200", null, "/some/path/mgform1.pdf", "Pending", receivedDate),
            },
                OtherMaterials = new List<MgForm>
            {
                new MgForm(2, "OtherMaterialA.pdf", "OriginalD.pdf", "1300", null, "/some/path/other1.pdf", "Pending", receivedDate),
            },
            };

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("0"))
                .Returns(new DocumentTypeInfo { Category = "Unknown", DocumentType = "Unknown" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1200"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.MapMaterialType("1300"))
                .Returns(new DocumentTypeInfo { Category = "Mapped Category B", DocumentType = "Mapped DocumentType B" });

            this.mockDocumentTypeMapper
                .Setup(mapper => mapper.GetDocumentTypesWithClassificationGroup())
                .Returns(new List<DocumentTypeGroup> { new() { Id = 1012, Category = "Mapped category A", Name = "Mapped name A", Group = "Mapped Group A" } });

            // Act
            List<CaseMaterial> result = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(unusedMaterialsResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            this.mockDocumentTypeMapper
                .Verify(mapper => mapper.MapMaterialType("1200"), Times.Exactly(1));

            this.mockDocumentTypeMapper
              .Verify(mapper => mapper.MapMaterialType("1300"), Times.Exactly(1));

            // Validate first item
            CaseMaterial mgForms = result.First();
            Assert.Equal(1, mgForms.Id);
            Assert.Equal("OriginalC.pdf", mgForms.OriginalFileName);
            Assert.Equal("MgFormA.pdf", mgForms.Subject);
            Assert.Equal(1200, mgForms.DocumentTypeId);
            Assert.Equal(1, mgForms.MaterialId);
            Assert.Equal("/some/path/mgform1.pdf", mgForms.Link);
            Assert.Equal("Mapped Category A", mgForms.Category);
            Assert.Equal("Mapped DocumentType A", mgForms.Type);
            Assert.False(mgForms.HasAttachments);
            Assert.Equal("Unused", mgForms.Status);
            Assert.Equal(receivedDate, mgForms.Date);
            Assert.False(result.First().IsReclassifiable);
            Assert.False(result.Last().IsReclassifiable);
        }

        /// <summary>
        /// Exhibit producers mock data.
        /// </summary>
        private static ExhibitProducersResponse BuildTestExhibitProducers()
        {
            return new ExhibitProducersResponse()
            {
                ExhibitProducers = new List<ExhibitProducer>()
            {
                new (1212, "some producer 1", false),
                new (1213, "some producer 2", false),
                new (1214, "some producer 3", false),
            },
            };
        }
    }
}
