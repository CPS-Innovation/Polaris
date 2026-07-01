# Implementation Plan: Add Witness Support to `UpdateStatementAsync`

**Repository:** `https://github.com/CPS-Innovation/Polaris`  
**Branch:** `feature/FC2-16120-MaterialsTab-Filter-Change`  
**Date:** 2025  
**Author:** GitHub Copilot  

---

## Status of Changes

| # | File | Status |
|---|---|---|
| 1 | `Common/Dto/Request/HouseKeeping/UpdateStatementRequest.cs` | ? Pending |
| 2 | `Cps.Fct.Hk.Ui.Interfaces/ICommunicationService.cs` | ? Pending |
| 3 | `Cps.Fct.Hk.Ui.Services/CommunicationService.cs` | ? Pending |
| 4 | `polaris-gateway/Functions/HouseKeeping/UpdateStatement.cs` | ? Pending |
| 5 | `Cps.Fct.Hk.Ui.Services/Validators/UpdateStatementRequestValidator.cs` | ? Pending |

---

## Background

The existing `CompleteReclassificationAsync` flow in `MaterialReclassificationOrchestrationService` supports an "add new witness" journey when reclassifying material to a Statement type. If the user does not find their witness in the dropdown, they can enter a new witness name, which is added to the case before the reclassification is submitted.

`UpdateStatementAsync` in `CommunicationService` currently has **no equivalent capability**. It passes `WitnessId` directly to the downstream API with no mechanism for creating a new witness first. Unlike `UpdateExhibit` (where a witness is optional), `UpdateStatement` **requires** a valid `WitnessId > 0` — meaning a user whose witness is not in the dropdown is **completely blocked** from updating a statement.

This document describes the changes required to bring add-witness support into the `UpdateStatement` flow.

---

## Key Difference vs `UpdateExhibit`

| | `UpdateExhibit` | `UpdateStatement` |
|---|---|---|
| Witness field in request | `ExistingProducerOrWitnessId` (`int?`, optional) | `WitnessId` (`int`, **required**) |
| Witness required? | No — producer can be free-text via `NewProducer` | **Yes — always required** |
| `WitnessId` mutable? | N/A | **No — currently init-only in record constructor** |
| Validator blocks add-witness? | No | **Yes — unconditional `WitnessId > 0` rule** |

---

## Current Flow

```
User picks witness from dropdown (WitnessId set)
        ?
UpdateStatement (gateway function)
        ?
UpdateStatementRequestValidator — WitnessId must be > 0 (unconditional)
        ?
CommunicationService.UpdateStatementAsync()
        ?
apiClient.UpdateStatementAsync(request, cmsAuthValues)
        ?
UpdateStatementResponse
```

---

## Target Flow (with Add Witness)

```
User selects "Add new witness" (WitnessId = 0, Witness.Surname present)
        ?
UpdateStatement (gateway function)
        ?
UpdateStatementRequestValidator
    ??? AddWitness() == true  ? validate Witness.FirstName + Witness.Surname
    ??? AddWitness() == false ? validate WitnessId > 0 (existing behaviour)
        ?
CommunicationService.UpdateStatementAsync()
        ?
    statement.AddWitness() == true?
    ??? YES ? WitnessService.AddWitnessAsync(urn, caseId, firstName, surname)
    ?             ? apiClient.AddWitnessAsync()
    ?             ? GetCaseWitnessesAsync() to resolve new ID
    ?             ? set statement.WitnessId = newWitnessId
    ??? NO  ? use statement.WitnessId as-is
        ?
apiClient.UpdateStatementAsync(request, cmsAuthValues)
        ?
UpdateStatementResponse
```

---

## Detailed Changes

### 1. `UpdateStatementRequest` ? Pending

**File:** `polaris-pipeline\Common\Dto\Request\HouseKeeping\UpdateStatementRequest.cs`  
**Project:** `Common` (`polaris-pipeline\Common\Common.csproj`)

Three sub-changes are required:

**a) Make `WitnessId` settable** — currently init-only as a primary constructor parameter. Use the same pattern `MaterialId` already uses in this record:

```csharp
// Before
public record UpdateStatementRequest(
    Guid id,
    [property: JsonPropertyName("caseId")] int CaseId,
    int materialIdentifier,
    [property: JsonPropertyName("witnessId")] int WitnessId,  // ? init-only
    [property: JsonPropertyName("statementDate")] DateOnly? StatementDate,
    [property: JsonPropertyName("statementNumber")] int StatementNumber,
    [property: JsonPropertyName("used")] bool Used)
       : BaseRequest(CorrespondenceId: id)
{
    [JsonPropertyName("materialId")]
    public int MaterialId { get; set; } = materialIdentifier;
    ...
}

// After
public record UpdateStatementRequest(
    Guid id,
    [property: JsonPropertyName("caseId")] int CaseId,
    int materialIdentifier,
    int witnessId,                                            // ? bare parameter
    [property: JsonPropertyName("statementDate")] DateOnly? StatementDate,
    [property: JsonPropertyName("statementNumber")] int StatementNumber,
    [property: JsonPropertyName("used")] bool Used)
       : BaseRequest(CorrespondenceId: id)
{
    [JsonPropertyName("materialId")]
    public int MaterialId { get; set; } = materialIdentifier;

    // Settable — same pattern as MaterialId
    [JsonPropertyName("witnessId")]
    public int WitnessId { get; set; } = witnessId;
    ...
}
```

**b) Add `Witness` property:**
```csharp
/// <summary>
/// Gets or sets the new witness details when the user is adding a witness not in the dropdown.
/// </summary>
[JsonPropertyName("witness")]
public WitnessRequest? Witness { get; set; }
```

**c) Add `AddWitness()` helper:**
```csharp
/// <summary>
/// Indicates if a new witness should be added based on request data.
/// True when WitnessId is 0 or not set but a witness surname is present.
/// </summary>
/// <returns>True if a new witness should be created before updating the statement.</returns>
public bool AddWitness() =>
    WitnessId <= 0
    && !string.IsNullOrWhiteSpace(Witness?.Surname);
```

> **Why `WitnessId <= 0` not `Witness?.WitnessId`:** `WitnessId` is a non-nullable `int` in the constructor (unlike `UpdateExhibitRequest` where `ExistingProducerOrWitnessId` is `int?`). The frontend will send `0` when no existing witness is selected.

---

### 2. `ICommunicationService` ? Pending

**File:** `Cps.Fct.Hk.Ui.Interfaces\ICommunicationService.cs` (line 198)  
**Project:** `Cps.Fct.Hk.Ui.Interfaces` (`Cps.Fct.Hk.Ui.Interfaces\Cps.Fct.Hk.Ui.Interfaces.csproj`)

```csharp
// Before
/// <param name="caseId">The ID of the case.</param>
/// <param name="statement">The update statement request.</param>
/// <param name="cmsAuthValues">Authorization values for CMS access.</param>
/// <param name="correspondenceId">correspondenceId.</param>
public Task<UpdateStatementResponse> UpdateStatementAsync(
    int caseId,
    UpdateStatementRequest statement,
    CmsAuthValues cmsAuthValues,
    Guid correspondenceId = default);

// After
/// <param name="caseId">The ID of the case.</param>
/// <param name="urn">The URN of the case.</param>
/// <param name="statement">The update statement request.</param>
/// <param name="cmsAuthValues">Authorization values for CMS access.</param>
/// <param name="correspondenceId">correspondenceId.</param>
public Task<UpdateStatementResponse> UpdateStatementAsync(
    int caseId,
    string urn,
    UpdateStatementRequest statement,
    CmsAuthValues cmsAuthValues,
    Guid correspondenceId = default);
```

**Why:** `WitnessService.AddWitnessAsync` requires a `urn` parameter. It is currently missing from the `UpdateStatement` code path but is already present in the gateway function's route (`urns/{caseUrn}/cases/{caseId}/materials/{materialId}/statement`).

---

### 3. `CommunicationService` ? Pending

**File:** `Cps.Fct.Hk.Ui.Services\CommunicationService.cs`  
**Project:** `Cps.Fct.Hk.Ui.Services` (`Cps.Fct.Hk.Ui.Services\Cps.Fct.Hk.Ui.Services.csproj`)

**a) Constructor change** — inject `IWitnessService`:
```csharp
// Before
public CommunicationService(
    ILogger<CommunicationService> logger,
    IMasterDataServiceClient apiClient,
    IDocumentTypeMapper documentTypeMapper,
    ICommunicationMapper communicationMapper)

// After
public CommunicationService(
    ILogger<CommunicationService> logger,
    IMasterDataServiceClient apiClient,
    IDocumentTypeMapper documentTypeMapper,
    ICommunicationMapper communicationMapper,
    IWitnessService witnessService)  // ? new dependency
```

> **Note:** If `UpdateExhibit` add-witness changes are implemented in the same PR, this constructor change only needs to be made once.

**b) Method change** — add `urn` parameter and add-witness pre-step:
```csharp
// Before
public async Task<UpdateStatementResponse> UpdateStatementAsync(
    int caseId,
    UpdateStatementRequest statement,
    CmsAuthValues cmsAuthValues,
    Guid correspondenceId = default)
{
    var request = new UpdateStatementRequest(
        correspondenceId == default ? Guid.NewGuid() : correspondenceId,
        caseId,
        statement.MaterialId,
        statement.WitnessId,
        statement.StatementDate,
        statement.StatementNumber,
        statement.Used);

    UpdateStatementResponse response = await this.apiClient
        .UpdateStatementAsync(request, cmsAuthValues).ConfigureAwait(false);

    return response;
}

// After
public async Task<UpdateStatementResponse> UpdateStatementAsync(
    int caseId,
    string urn,                      // ? new
    UpdateStatementRequest statement,
    CmsAuthValues cmsAuthValues,
    Guid correspondenceId = default)
{
    // Step 1: Add witness first if user entered a new witness
    // (sequential — statement update depends on the resolved witnessId)
    if (statement.AddWitness())
    {
        this.logger.LogInformation(
            $"{LoggingConstants.HskUiLogPrefix} New witness detected for statement update, adding witness for caseId [{caseId}]");

        int? newWitnessId = await this.witnessService.AddWitnessAsync(
            urn,
            caseId,
            statement.Witness!.FirstName,
            statement.Witness.Surname,
            cmsAuthValues,
            correspondenceId).ConfigureAwait(false);

        statement.WitnessId = newWitnessId ?? statement.WitnessId;
    }

    // Step 2: Proceed with statement update (unchanged from current implementation)
    var request = new UpdateStatementRequest(
        correspondenceId == default ? Guid.NewGuid() : correspondenceId,
        caseId,
        statement.MaterialId,
        statement.WitnessId,         // ? now populated with new or existing id
        statement.StatementDate,
        statement.StatementNumber,
        statement.Used);

    UpdateStatementResponse response = await this.apiClient
        .UpdateStatementAsync(request, cmsAuthValues).ConfigureAwait(false);

    return response;
}
```

---

### 4. `UpdateStatement.cs` ? Pending

**File:** `polaris-gateway\Functions\HouseKeeping\UpdateStatement.cs`  
**Project:** `polaris-gateway` (`polaris-gateway\polaris-gateway.csproj`)

`caseUrn` is already part of the route (`urns/{caseUrn}/cases/{caseId}/materials/{materialId}/statement`) but is not currently extracted in the `Run` signature.

```csharp
// Before
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RestApi.UpdateStatement)] HttpRequest request,
    int caseId,
    int materialId)

// After
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RestApi.UpdateStatement)] HttpRequest request,
    string caseUrn,      // ? new — already in route definition
    int caseId,
    int materialId)
```

```csharp
// Before
UpdateStatementResponse result = await this.communicationService.UpdateStatementAsync(
    caseId,
    updateStatementRequest,
    cmsAuthValues).ConfigureAwait(true);

// After
UpdateStatementResponse result = await this.communicationService.UpdateStatementAsync(
    caseId,
    caseUrn,             // ? new
    updateStatementRequest,
    cmsAuthValues).ConfigureAwait(true);
```

---

### 5. `UpdateStatementRequestValidator` ? Pending

**File:** `Cps.Fct.Hk.Ui.Services\Validators\UpdateStatementRequestValidator.cs`  
**Project:** `Cps.Fct.Hk.Ui.Services` (`Cps.Fct.Hk.Ui.Services\Cps.Fct.Hk.Ui.Services.csproj`)

> ?? **Critical change.** The current unconditional `WitnessId > 0` rule will reject add-witness requests at the gateway **before they ever reach the service**. Without this change, the entire add-witness flow is blocked.

```csharp
// Before — unconditional, blocks add-witness flow entirely
public UpdateStatementRequestValidator()
{
    this.RuleFor(x => x.MaterialId)
        .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

    this.RuleFor(x => x.WitnessId)
        .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

    this.RuleFor(x => x.StatementNumber)
        .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

    this.RuleFor(x => x.Used)
        .NotNull().WithMessage("{PropertyName} is required.");
}

// After — WitnessId validation is conditional on whether a new witness is being added
public UpdateStatementRequestValidator()
{
    this.RuleFor(x => x.MaterialId)
        .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

    // Only require a valid WitnessId when not adding a new witness
    this.When(x => !x.AddWitness(), () =>
    {
        this.RuleFor(x => x.WitnessId)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");
    });

    // When adding a new witness, validate the witness name fields instead
    this.When(x => x.AddWitness(), () =>
    {
        this.RuleFor(x => x.Witness!.FirstName)
            .NotEmpty()
            .WithMessage("Witness first name is required when adding a new witness.");

        this.RuleFor(x => x.Witness!.Surname)
            .NotEmpty()
            .WithMessage("Witness surname is required when adding a new witness.");
    });

    this.RuleFor(x => x.StatementNumber)
        .NotEmpty().WithMessage("{PropertyName} is required.")
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

    this.RuleFor(x => x.Used)
        .NotNull().WithMessage("{PropertyName} is required.");
}
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Add witness **before** statement update (sequential) | Statement update depends on the resolved `WitnessId` — same ordering constraint as `CompleteReclassificationAsync` |
| Reuse `WitnessService.AddWitnessAsync` unchanged | Avoids duplication; already handles case-lock check, API call, and new witness ID resolution |
| `AddWitness()` helper on the request object | Keeps orchestration decisions co-located with data — mirrors `CompleteReclassificationRequest.AddWitness()` and `UpdateExhibitRequest.AddWitness()` |
| `urn` added to `UpdateStatementAsync` signature | Required by `AddWitnessAsync` — currently missing from this code path |
| `WitnessId` made settable (Change 1a) | Required so `CommunicationService` can inject the resolved new witness ID before building the API request |
| `WitnessId` validation wrapped in `When(!AddWitness)` | Without this the validator rejects add-witness requests before they reach the service — unique requirement vs `UpdateExhibit` |
| Strategy pattern **not used** | Only one add-witness implementation exists (`WitnessService`); conditional pre-step is simpler and sufficient |

---

## What Is Not Changing

- `WitnessService.AddWitnessAsync` — reused as-is
- `WitnessRequest` record — reused as-is
- The downstream `apiClient.UpdateStatementAsync` call — unchanged
- All existing statement update behaviour when `AddWitness()` returns `false`
- `StatementNumber`, `MaterialId`, `Used` validation rules — unchanged
