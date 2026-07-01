# Unit Test Update - Material Type Exclusion

## Overview
Added comprehensive unit test coverage for the material type exclusion feature (PE3, PE4, DREP) in `GetCaseMaterials`.

## Test Added

### Test Name
`Run_ExcludesPE3PE4AndDREPMaterialTypes_FromUsedMgForms`

### Location
`polaris-gateway.tests/Functions/HouseKeeping/GetCaseMaterialsTests.cs`

### Purpose
Verifies that material types PE3 (1053), PE4 (1054), and DREP (1055) are correctly excluded from Used MG Forms when processing case materials.

### Test Coverage

The test validates:

1. **Exclusion Logic**: Creates 4 MG Forms:
   - PE3 (MaterialType: "1053") - Should be excluded
   - PE4 (MaterialType: "1054") - Should be excluded
   - DREP (MaterialType: "1055") - Should be excluded
   - Valid MG Form (MaterialType: "1202") - Should be included

2. **Expected Behavior**:
   - Only the valid MG Form (1202) is passed to `MapUsedMgFormsToCaseMaterials`
   - PE3, PE4, and DREP are filtered out before mapping
   - Final result contains 2 items: 1 communication + 1 valid MG Form

3. **Verification**:
   - ✅ Result count is correct (2 items)
   - ✅ Valid MG Form is present in result
   - ✅ PE3 Form is NOT in result
   - ✅ PE4 Form is NOT in result
   - ✅ DREP Form is NOT in result
   - ✅ Service method called exactly once with filtered data

## Test Results

### Before Changes
- Total Tests: 313
- Status: All passing

### After Changes
- Total Tests: 314 (1 new test added)
- Status: All passing
- New Test: ✅ PASS

## Code Tested

The test validates the following code in `GetCaseMaterials.cs`:

```csharp
if (usedMgForms.MgForms != null && usedMgForms.MgForms.Count != 0)
{
	usedMgForms.MgForms.RemoveAll(mgForm => 
		MaterialTypeIds.ExcludedFromUsedMgForms.Contains(mgForm.MaterialType));

	allCaseMaterials?.AddRange(
		this.caseMaterialService.MapUsedMgFormsToCaseMaterials(usedMgForms));
}
```

## Dependencies Tested

### Constants Used
- `MaterialTypeIds.ExcludedFromUsedMgForms` = `["1053", "1054", "1055"]`

### Services Mocked
- `ICommunicationService`
- `ICaseMaterialService`

### Data Types
- `MgForm` with MaterialType property (string)
- `CaseMaterial` with Subject property (string)
- `UsedMgFormsResponse` with MgForms collection

## Test Methodology

1. **Arrange**: Set up mock data with both excluded and included material types
2. **Act**: Call `GetCaseMaterials.Run()` with test data
3. **Assert**: 
   - Verify filtering occurred before service call
   - Verify correct items in final result
   - Verify excluded items NOT in result

## Integration with Constants

The test validates that the implementation correctly uses:
- `MaterialTypeIds.PE3` = "1053"
- `MaterialTypeIds.PE4` = "1054"
- `MaterialTypeIds.DREP` = "1055"
- `MaterialTypeIds.ExcludedFromUsedMgForms` array

## Benefits

1. **Regression Prevention**: Ensures material type exclusion logic continues to work
2. **Documentation**: Test serves as living documentation of expected behavior
3. **Confidence**: Provides confidence when refactoring or modifying related code
4. **Specification**: Clearly defines which material types should be excluded

## Related Files

### Production Code
- `polaris-gateway/Functions/HouseKeeping/GetCaseMaterials.cs`
- `Cps.Fct.Hk.Ui.Services/Constants/MaterialTypeIds.cs`

### Test Code
- `polaris-gateway.tests/Functions/HouseKeeping/GetCaseMaterialsTests.cs`

---
**Status**: ✅ COMPLETE  
**Branch**: feature/FCT2-19748-CommsTabMappingMissingData  
**Test Count**: 314 tests (1 new)  
**All Tests**: ✅ PASSING
