// <copyright file="HkUiFunctionsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace HkuiFunctionsIntegrationTests;

using System.Text.Json;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Integration.Tests.TestUtilities;
using HkUiE2ETests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Integration tests for Housekeeping Ui Functions backend service,
/// verifying that messages can be reclassified in CMS.
/// </summary>
public class HkUiFunctionsTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HkUiFunctionsTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public HkUiFunctionsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    /// <summary>
    /// Tests that the authentication context is retrieved after successful DDEI username/password authentication.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Authenticate_Should_Return_Authentication_Context()
    {
        // Arrange
        string? username = this.Configuration?.GetValue<string>("MasterDataServiceClient:CredentialName");
        string? password = this.Configuration?.GetValue<string>("MasterDataServiceClient:CredentialPassword");
        Assert.False(string.IsNullOrWhiteSpace(username), "MasterDataServiceClient:CredentialName configuration is missing.");
        Assert.False(string.IsNullOrWhiteSpace(password), "MasterDataServiceClient:CredentialPassword configuration is missing.");

        AuthenticationRequest authenticationRequest = new(
            CorrespondenceId: Guid.NewGuid(),
            Username: username,
            Password: password);

        // Act
        AuthenticationContext result = await this.AuthenticateAsync(authenticationRequest).ConfigureAwait(true);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.Cookies), "Authentication cookies should not be null or empty.");
        Assert.False(string.IsNullOrWhiteSpace(result.Token), "Authentication token should not be null or empty.");
        Assert.True(result.ExpiryTime > DateTimeOffset.UtcNow, "Authentication expiry time should be in the future.");
    }

    /// <summary>
    /// Tests that case information is retrieved correctly for a given case Id.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseInfo_Should_Return_Correct_GetCaseSummary_Data()
    {
        // Arrange
        this.ValidateTestDependencies();
        (int caseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext);

        var loggerMock = new Mock<ILogger<GetCaseInfo>>();

        // Instantiate the function with dependencies
        var function = new GetCaseInfo(
            logger: loggerMock.Object,
            caseInfoService: this.caseInfoService!);

        // Setup the expected CaseSummary result
        var expectedCaseSummary = new CaseSummaryResponse(
            caseId,
            "16SL1235025",
            "Do Not Use",
            "INTEGRATION TEST",
            1,
            "Hull CJU");

        // Act
        IActionResult response = await function.Run(request, caseId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        CaseSummaryResponse result = Assert.IsType<CaseSummaryResponse>(okResult.Value);

        Assert.Equal(expectedCaseSummary.CaseId, result.CaseId);
        Assert.Equal(expectedCaseSummary.Urn, result.Urn);
        Assert.Equal(expectedCaseSummary.LeadDefendantFirstNames, result.LeadDefendantFirstNames);
        Assert.Equal(expectedCaseSummary.LeadDefendantSurname, result.LeadDefendantSurname);
        Assert.Equal(expectedCaseSummary.NumberOfDefendants, result.NumberOfDefendants);
        Assert.Equal(expectedCaseSummary.UnitName, result.UnitName);
    }

    /// <summary>
    /// Tests that case materials are retrieved correctly for a given case Id.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseMaterials_Should_Return_Correct_Case_Materials_List()
    {
        // Arrange
        await this.ResetAllCommunications().ConfigureAwait(true);

        this.ValidateTestDependencies();
        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext);

        var loggerMock = new Mock<ILogger<GetCaseMaterials>>();

        // Instantiate the function with dependencies
        var function = new GetCaseMaterials(
            logger: loggerMock.Object,
            communicationService: this.communicationService!,
            caseMaterialService: this.caseMaterialService!);

        // Setup the expected CaseMaterial result
        List<CaseMaterial> expectedCaseMaterials = CaseMaterialTestData.GetExpectedCaseMaterials();

        // Act
        await Task.Delay(testDelay);
        IActionResult response = await function.Run(request, caseId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        List<CaseMaterial> result = Assert.IsType<List<CaseMaterial>>(okResult.Value);

        Assert.Equal(expectedCaseMaterials.Count, result.Count);

        foreach (CaseMaterial expected in expectedCaseMaterials)
        {
            CaseMaterial? actual = result.FirstOrDefault(x => x.MaterialId == expected.MaterialId);
            Assert.NotNull(actual); // ensure match found

            Assert.Equal(expected.OriginalFileName, actual.OriginalFileName);
            Assert.Equal(expected.Subject, actual.Subject);
            Assert.Equal(expected.DocumentTypeId, actual.DocumentTypeId);
            Assert.Equal(expected.MaterialId, actual.MaterialId);
            Assert.EndsWith(expected.OriginalFileName, actual.Link); // only suffix is asserted
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.HasAttachments, actual.HasAttachments);
            Assert.Equal(expected.ReadStatus, actual.ReadStatus);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Direction, actual.Direction);
            Assert.Equal(expected.Party, actual.Party);
            Assert.Equal(expected.Date?.Date, actual.Date?.Date);
            Assert.Equal(expected.RecordedDate, actual.RecordedDate);
        }
    }

    /// <summary>
    /// Tests that bulk set unused reclassifies the given collection of material Ids correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task BulkSetUnused_Should_Reclassify_User_Selected_Case_Materials_Correctly()
    {
        // Arrange
        await this.ResetAllCommunications().ConfigureAwait(true);

        this.ValidateTestDependencies();
        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);

        var loggerMockBulkSetUnused = new Mock<ILogger<BulkSetUnused>>();

        var bulkSetUnusedFunction = new BulkSetUnused(
            logger: loggerMockBulkSetUnused.Object,
            bulkSetUnusedService: this.bulkSetUnusedService!);

        IReadOnlyCollection<BulkSetUnusedRequest> requestBody = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest
            {
                materialId = 8806659,
                subject = "Unused 11",
            },
            new BulkSetUnusedRequest
            {
                materialId = 8806660,
                subject = "Unused12",
            },
        };

        string jsonContent = JsonSerializer.Serialize(requestBody);

        HttpRequest bulkSetUnusedRequest = this.CreateHttpRequestWithCookie(caseId, authContext, jsonContent, "POST");

        var expectedBulkSetUnusedResponse = new BulkSetUnusedResponse
        {
            Status = "success",
            Message = "All materials were successfully reclassified.",
            ReclassifiedMaterials = new List<ReclassifiedMaterial>
            {
                new ReclassifiedMaterial
                {
                    MaterialId = 8806659,
                    Subject = "Unused 11",
                },
                new ReclassifiedMaterial
                {
                    MaterialId = 8806660,
                    Subject = "Unused12",
                },
            },
            FailedMaterials = new List<FailedMaterial>(),
        };

        // Act
        await Task.Delay(testDelay);
        IActionResult bulkSetUnusedResponse = await bulkSetUnusedFunction.Run(bulkSetUnusedRequest, caseId);

        // Assert
        OkObjectResult okResultBulkSetUnused = Assert.IsType<OkObjectResult>(bulkSetUnusedResponse);
        BulkSetUnusedResponse resultBulkSetUnused = Assert.IsType<BulkSetUnusedResponse>(okResultBulkSetUnused.Value);

        Assert.NotNull(resultBulkSetUnused);
        Assert.NotNull(resultBulkSetUnused.ReclassifiedMaterials);
        Assert.Equal(2, resultBulkSetUnused.ReclassifiedMaterials.Count);
        Assert.Empty(resultBulkSetUnused.FailedMaterials);

        Assert.Equal(expectedBulkSetUnusedResponse.ReclassifiedMaterials[0].MaterialId, resultBulkSetUnused.ReclassifiedMaterials[0].MaterialId);
        Assert.Equal(expectedBulkSetUnusedResponse.ReclassifiedMaterials[0].Subject, resultBulkSetUnused.ReclassifiedMaterials[0].Subject);
        Assert.Equal(expectedBulkSetUnusedResponse.ReclassifiedMaterials[1].MaterialId, resultBulkSetUnused.ReclassifiedMaterials[1].MaterialId);
        Assert.Equal(expectedBulkSetUnusedResponse.ReclassifiedMaterials[1].Subject, resultBulkSetUnused.ReclassifiedMaterials[1].Subject);
        Assert.Equal(expectedBulkSetUnusedResponse.FailedMaterials, resultBulkSetUnused.FailedMaterials);

        // Verify the bulk set unused case materials
        List<CaseMaterial> expectedBulkSetUnusedCaseMaterials = CaseMaterialTestDataBulkSetUnused.GetExpectedCaseMaterials();
        HttpRequest getCaseMaterialsRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var loggerMockGetCaseMaterials = new Mock<ILogger<GetCaseMaterials>>();

        var getCaseMaterialsFunction = new GetCaseMaterials(
            logger: loggerMockGetCaseMaterials.Object,
            communicationService: this.communicationService!,
            caseMaterialService: this.caseMaterialService!);

        await Task.Delay(testDelay);
        IActionResult responseGetCaseMaterialsFunction = await getCaseMaterialsFunction.Run(getCaseMaterialsRequest, caseId);

        OkObjectResult okResultGetCaseMaterials = Assert.IsType<OkObjectResult>(responseGetCaseMaterialsFunction);
        List<CaseMaterial> resultGetCaseMaterials = Assert.IsType<List<CaseMaterial>>(okResultGetCaseMaterials.Value);

        foreach (CaseMaterial expected in expectedBulkSetUnusedCaseMaterials)
        {
            CaseMaterial? actual = resultGetCaseMaterials.FirstOrDefault(x => x.MaterialId == expected.MaterialId);
            Assert.NotNull(actual);

            Assert.Equal(expected.OriginalFileName, actual.OriginalFileName);
            Assert.Equal(expected.Subject, actual.Subject);
            Assert.Equal(expected.DocumentTypeId, actual.DocumentTypeId);
            Assert.Equal(expected.MaterialId, actual.MaterialId);
            Assert.EndsWith(expected.OriginalFileName, actual.Link);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.HasAttachments, actual.HasAttachments);
            Assert.Equal(expected.ReadStatus, actual.ReadStatus);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Direction, actual.Direction);
            Assert.Equal(expected.Party, actual.Party);
            Assert.Equal(expected.Date?.Date, actual.Date?.Date);
            Assert.Equal(expected.RecordedDate, actual.RecordedDate);
        }
    }

    /// <summary>
    /// Tests that uma reclassify function, reclassifies all the unused materials for the case correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UmaReclassify_Should_Reclassify_All_Case_Materials_Correctly()
    {
        // Arrange
        await this.ResetAllCommunications().ConfigureAwait(true);

        this.ValidateTestDependencies();
        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        HttpRequest umaReclassifyRequest = this.CreateHttpRequestWithCookie(caseId, authContext, "POST");

        var loggerMockUmaReclassify = new Mock<ILogger<UmaReclassify>>();

        var umaReclassifyFunction = new UmaReclassify(
            logger: loggerMockUmaReclassify.Object,
            communicationService: this.communicationService!,
            umaReclassifyService: this.umaReclassifyService!,
            bulkSetUnusedService: this.bulkSetUnusedService!);

        // Act
        await Task.Delay(testDelay);
        IActionResult umaReclassifyResponse = await umaReclassifyFunction.Run(umaReclassifyRequest, caseId);

        // Assert
        OkObjectResult okResultUmaReclassify = Assert.IsType<OkObjectResult>(umaReclassifyResponse);
        BulkSetUnusedResponse resultUmaReclassify = Assert.IsType<BulkSetUnusedResponse>(okResultUmaReclassify.Value);

        Assert.NotNull(resultUmaReclassify);
        Assert.NotNull(resultUmaReclassify.ReclassifiedMaterials);
        Assert.Equal(12, resultUmaReclassify.ReclassifiedMaterials.Count);
        Assert.Empty(resultUmaReclassify.FailedMaterials);

        // Verify the uma reclassified case materials
        List<CaseMaterial> expectedUmaReclassifyCaseMaterials = CaseMaterialTestDataUmaReclassify.GetExpectedCaseMaterials();
        HttpRequest getCaseMaterialsRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var loggerMockGetCaseMaterials = new Mock<ILogger<GetCaseMaterials>>();

        var getCaseMaterialsFunction = new GetCaseMaterials(
            logger: loggerMockGetCaseMaterials.Object,
            communicationService: this.communicationService!,
            caseMaterialService: this.caseMaterialService!);

        await Task.Delay(testDelay);
        IActionResult responseGetCaseMaterialsFunction = await getCaseMaterialsFunction.Run(getCaseMaterialsRequest, caseId);

        OkObjectResult okResultGetCaseMaterials = Assert.IsType<OkObjectResult>(responseGetCaseMaterialsFunction);
        List<CaseMaterial> resultGetCaseMaterials = Assert.IsType<List<CaseMaterial>>(okResultGetCaseMaterials.Value);

        foreach (CaseMaterial expected in expectedUmaReclassifyCaseMaterials)
        {
            CaseMaterial? actual = resultGetCaseMaterials.FirstOrDefault(x => x.MaterialId == expected.MaterialId);
            Assert.NotNull(actual);

            Assert.Equal(expected.OriginalFileName, actual.OriginalFileName);
            Assert.Equal(expected.Subject, actual.Subject);
            Assert.Equal(expected.DocumentTypeId, actual.DocumentTypeId);
            Assert.Equal(expected.MaterialId, actual.MaterialId);
            Assert.EndsWith(expected.OriginalFileName, actual.Link);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.HasAttachments, actual.HasAttachments);
            Assert.Equal(expected.ReadStatus, actual.ReadStatus);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Direction, actual.Direction);
            Assert.Equal(expected.Party, actual.Party);
            Assert.Equal(expected.Date?.Date, actual.Date?.Date);
            Assert.Equal(expected.RecordedDate, actual.RecordedDate);
        }
    }

    /// <summary>
    /// Tests that case materials are renamed correctly for a given material Id.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RenameMaterial_Should_Return_Correctly_And_Reset_Test_Data()
    {
        // Arrange
        this.ValidateTestDependencies();
        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);

        int materialIdToRename = 8806660;
        string originalSubject = "Unused12";
        string newSubject = "Renamed Test Subject";

        var loggerMock = new Mock<ILogger<RenameMaterial>>();

        // Instantiate the function with dependencies
        var function = new RenameMaterial(
            logger: loggerMock.Object,
            communicationService: this.communicationService!,
            renameMaterialRequestValidator: this.renameMaterialRequestValidator!);

        try
        {
            var requestBody = new RenameMaterialRequest(
                id: Guid.NewGuid(),
                materialId: materialIdToRename,
                subject: newSubject);

            string jsonContent = JsonSerializer.Serialize(requestBody);
            HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext, jsonContent, "PATCH");

            // Setup the expected RenameMaterialResponse result
            var expectedRenamedMaterial = new RenameMaterialResponse(
                new RenameMaterialData
                {
                    Id = materialIdToRename,
                });

            // Act
            IActionResult response = await function.Run(request, caseId, materialIdToRename);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
            RenameMaterialResponse result = Assert.IsType<RenameMaterialResponse>(okResult.Value);

            Assert.NotNull(result);
            Assert.NotNull(result.RenameMaterialData);
            Assert.Equal(expectedRenamedMaterial.RenameMaterialData.Id, result.RenameMaterialData.Id);
        }
        finally
        {
            try
            {
                await Task.Delay(testDelay);
                var resetRequest = new RenameMaterialRequest(Guid.NewGuid(), materialIdToRename, originalSubject);
                string resetJson = JsonSerializer.Serialize(resetRequest);
                HttpRequest resetHttpRequest = this.CreateHttpRequestWithCookie(caseId, authContext, resetJson, "PATCH");

                await function.Run(resetHttpRequest, caseId, materialIdToRename);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                this.testOutputHelper?.WriteLine($"Failed to reset material subject: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Tests that  GetStatementsForWitness returns expected data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetStatementsForWitness_Should_Return_Expected_Data()
    {
        // Arrange
        this.ValidateTestDependencies();
        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        int witnessId = 2087306;
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext);

        var loggerMock = new Mock<ILogger<GetCaseWitnessStatements>>();

        // Instantiate the function with dependencies
        var function = new GetCaseWitnessStatements(
            logger: loggerMock.Object,
            witnessService: this.witnessService!);

        // Setup the expected CaseSummary result
        var expectedWitnessStatements = new WitnessStatementsResponse()
        {
            WitnessStatements =
            [
                new (Id: 2353512, StatementNumber: 7),
            ],
        };

        // Act
        IActionResult response = await function.Run(request, caseId, witnessId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        WitnessStatementsResponse result = Assert.IsType<WitnessStatementsResponse>(okResult.Value);

        Assert.Equal(expectedWitnessStatements.WitnessStatements.First().Id, result.WitnessStatements?.Last().Id);
    }

    /// <summary>
    /// Tests that case defendants are retrieved correctly for a given case Id.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseDefendants_Should_Return_Correct_Case_Defendants_List()
    {
        // Arrange
        this.ValidateTestDependencies();
        (int caseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext);

        var loggerMock = new Mock<ILogger<GetCaseDefendants>>();

        // Instantiate the function with dependencies
        var function = new GetCaseDefendants(
            logger: loggerMock.Object,
            caseDefendantsService: this.caseDefendantsService!);

        // Act
        IActionResult response = await function.Run(request, caseId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        DefendantsResponse result = Assert.IsType<DefendantsResponse>(okResult.Value);

        Assert.NotNull(result.Defendants);
        Assert.Single(result.Defendants);

        Defendant defendant = result.Defendants.First();

        Assert.Equal(2781991, defendant.Id);
        Assert.Equal(2159815, defendant.CaseId);
        Assert.Equal(1, defendant.ListOrder);
        Assert.Equal("Person", defendant.Type);
        Assert.Equal("Do Not Use", defendant.FirstNames);
        Assert.Equal("INTEGRATION TEST", defendant.Surname);
        Assert.Equal("Male", defendant.PersonalDetail?.Gender);
        Assert.Equal("English", defendant.PersonalDetail?.PreferredCorrespondenceLanguage);

        Assert.NotNull(defendant.Offences);
        Assert.Single(defendant.Offences);

        Offence offence = defendant.Offences.First();
        Assert.Equal(2265152, offence.Id);
        Assert.Equal("TH68013", offence.Code);
        Assert.Equal("Theft of motor vehicle", offence.Description);
        Assert.Equal("NO_PLEA", offence.LatestPlea);
        Assert.Equal("NO_VERDICT", offence.LatestVerdict);
    }

    /// <summary>
    /// Tests that GetExhibitProducers returns expected data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetExhibitProducers_Should_Return_Expected_Data()
    {
        // Arrange
        this.ValidateTestDependencies();

        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();

        // Dedicated case id for get exhibitproducers.
        caseId = 2165838;
        int producerId = 2798677;
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext);

        var loggerMock = new Mock<ILogger<GetCaseExhibitProducers>>();

        // Instantiate the function with dependencies
        var function = new GetCaseExhibitProducers(
            logger: loggerMock.Object,
            communicationService: this.communicationService!,
            witnessService: this.witnessService!);

        // Setup the expected producer.
        var expectedProducer = new ExhibitProducer(Id: producerId, Name: "PC White", false);

        // Act
        IActionResult response = await function.Run(request, caseId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        ExhibitProducersResponse result = Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producer = result?.ExhibitProducers?.Find(x => x.Id == expectedProducer.Id);

        Assert.Equal(expectedProducer.Id, producer?.Id);
        Assert.Equal(expectedProducer.Name, producer?.Name);
    }

    /// <summary>
    /// Tests that CompleteReclassification returns expected data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CompleteReclassification_Should_FullyReclassifyCaseMaterial()
    {
        // Arrange
        this.ValidateTestDependencies();
        (int caseId, _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);

        var loggerMock = new Mock<ILogger<CompleteReclassification>>();
        int materialId = 2353509;
        int documentTypeid = 1200;

        var reclassifyStatementRequest = new ReclassifyStatementRequest()
        {
            Witness = 2087306,
            StatementNo = 5,
            Date = new DateOnly(2025, 06, 01),
        };

        var reclassifyCaseMaterialRequest = new ReclassifyCaseMaterialRequest(
            urn: "16SL1235025",
            classification: "STATEMENT",
            materialId: materialId,
            documentTypeId: documentTypeid,
            used: true,
            subject: "Statement subject",
            statementRequest: reclassifyStatementRequest);

        // Instantiate the function with dependencies
        var function = new CompleteReclassification(
            logger: loggerMock.Object,
            this.materialReclassificationOrchestrationService!,

            new Cps.Fct.Hk.Ui.Services.Validators.CompleteReclassificationRequestValidator());

        var requestBody = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest, null, null);

        string jsonContent = JsonSerializer.Serialize(requestBody);
        HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext, jsonContent, "POST");

        // Act
        IActionResult response = await function.Run(request, caseId, 2353509);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        CompleteReclassificationResponse result = Assert.IsType<CompleteReclassificationResponse>(okResult.Value);

        var operationResults = (CompleteReclassificationResponse)okResult.Value;
        Assert.True(operationResults.overallSuccess);
        Assert.Equal("Success", operationResults.status);

        ReclassificationResponse reclassificationResultData = (ReclassificationResponse)result.reclassificationResult.ResultData!;
        Assert.Equal(materialId, reclassificationResultData.ReclassifyCommunication.Id);
        Assert.Equal("ReclassifyCaseMaterial", result.reclassificationResult.OperationName);
        Assert.Null(result.addWitnessResult);
        Assert.Null(result.actionPlanResult);
    }

    /// <summary>
    /// Tests updateStatement function updates statement correctly, and reset test data back.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateStatement_Shoud_Update_Statemment_And_Reset_Data()
    {
        // Arrange
        this.ValidateTestDependencies();

        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);

        int materialId = 2353509;

        var loggerMock = new Mock<ILogger<UpdateStatement>>();

        var function = new UpdateStatement(
            loggerMock.Object,
            this.communicationService!,
            this.updateStatementRequestValidator!);

        try
        {
            var requestBody = new UpdateStatementRequest(
                id: Guid.NewGuid(),
                CaseId: 2021599,
                materialIdentifier: materialId,
                WitnessId: 2087307,
                StatementDate: new DateOnly(2025, 10, 17),
                StatementNumber: 30,
                Used: true);

            string jsonContent = JsonSerializer.Serialize(requestBody);
            HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext, jsonContent, "PATCH");

            UpdateStatementResponse? expectedResponse = new UpdateStatementResponse(
                new UpdateStatementData
                {
                    Id = materialId,
                });

            // Act
            IActionResult response = await function.Run(request, caseId, materialId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
            UpdateStatementResponse result = Assert.IsType<UpdateStatementResponse>(okResult.Value);

            Assert.NotNull(result);
            Assert.NotNull(result.UpdateStatementData);
            Assert.Equal(expectedResponse?.UpdateStatementData?.Id, result.UpdateStatementData.Id);
        }
        finally
        {
            await Task.Delay(testDelay);

            var resetRequestBody = new UpdateStatementRequest(
               id: Guid.NewGuid(),
               CaseId: 2021599,
               materialIdentifier: materialId,
               WitnessId: 2087307,
               StatementDate: new DateOnly(2025, 10, 17),
               StatementNumber: 6,
               Used: true);

            string resetJsonContent = JsonSerializer.Serialize(resetRequestBody);
            HttpRequest resetHttpRequest = this.CreateHttpRequestWithCookie(caseId, authContext, resetJsonContent, "PATCH");
            await function.Run(resetHttpRequest, caseId, materialId);
        }
    }

    /// <summary>
    /// Tests UpdateExhibit function updates exhibit correctly, and reset test data back.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateExhibit_Shoud_Update_Exhibit_And_Reset_Data()
    {
        // Arrange
        this.ValidateTestDependencies();

        (int caseId, int testDelay, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);

        int materialId = 2353512;

        var loggerMock = new Mock<ILogger<UpdateExhibit>>();

        var function = new UpdateExhibit(
            loggerMock.Object,
            this.communicationService!,
            this.updateExhibitRequestValidator!);

        try
        {
            var requestBody = new UpdateExhibitRequest(
                id: Guid.NewGuid(),
                caseIdentifier: 2021599,
                DocumentType: 1042,
                Item: "new-item",
                materialIdentifier: materialId,
                Reference: "some-ref",
                Subject: "some-item",
                Used: true,
                NewProducer: "new-producer",
                ExistingProducerOrWitnessId: null);

            string jsonContent = JsonSerializer.Serialize(requestBody);
            HttpRequest request = this.CreateHttpRequestWithCookie(caseId, authContext, jsonContent, "PATCH");

            UpdateExhibitResponse? expectedResponse = new UpdateExhibitResponse(
                new UpdateExhibitData
                {
                    Id = materialId,
                });

            // Act
            IActionResult response = await function.Run(request, caseId, materialId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
            UpdateExhibitResponse result = Assert.IsType<UpdateExhibitResponse>(okResult.Value);

            Assert.NotNull(result);
            Assert.NotNull(result.UpdateExhibitData);
            Assert.Equal(expectedResponse?.UpdateExhibitData?.Id, result.UpdateExhibitData.Id);
        }
        finally
        {
            await Task.Delay(testDelay);

            var resetRequestBody = new UpdateExhibitRequest(
                id: Guid.NewGuid(),
                caseIdentifier: 2021599,
                DocumentType: 1042,
                Item: "some-item",
                materialIdentifier: materialId,
                Reference: "some-ref",
                Subject: "some-item",
                Used: true,
                NewProducer: "producer",
                ExistingProducerOrWitnessId: null);

            string resetJsonContent = JsonSerializer.Serialize(resetRequestBody);
            HttpRequest resetHttpRequest = this.CreateHttpRequestWithCookie(caseId, authContext, resetJsonContent, "PATCH");
            await function.Run(resetHttpRequest, caseId, materialId);
        }
    }
}
