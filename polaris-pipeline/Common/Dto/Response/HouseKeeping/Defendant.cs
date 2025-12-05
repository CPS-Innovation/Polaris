// <copyright file="Defendant.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

/// <summary>
/// Represents a defendant involved in a legal case.
/// </summary>
public record Defendant(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("caseId")] int CaseId,
    [property: JsonPropertyName("listOrder")] int? ListOrder,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("firstNames")] string FirstNames,
    [property: JsonPropertyName("surname")] string Surname,
    [property: JsonPropertyName("dob")] DateTime? Dob,
    [property: JsonPropertyName("policeRemandStatus")] string? PoliceRemandStatus,
    [property: JsonPropertyName("youth")] bool? Youth,
    [property: JsonPropertyName("custodyTimeLimit")] string? CustodyTimeLimit,
    [property: JsonPropertyName("offences")] List<Offence> Offences,
    [property: JsonPropertyName("charges")] List<Charge> Charges,
    [property: JsonPropertyName("proposedCharges")] List<ProposedCharge> ProposedCharges,
    [property: JsonPropertyName("nextHearing")] object? NextHearing,
    [property: JsonPropertyName("defendantPcdReview")] object DefendantPcdReview,
    [property: JsonPropertyName("solicitor")] object? Solicitor,
    [property: JsonPropertyName("personalDetail")] PersonalDetail PersonalDetail);

/// <summary>
/// Represents an offence associated with a defendant.
/// </summary>
public record Offence(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("listOrder")] int? ListOrder,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("active")] string Active,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("fromDate")] string? FromDate,
    [property: JsonPropertyName("toDate")] string? ToDate,
    [property: JsonPropertyName("latestPlea")] string LatestPlea,
    [property: JsonPropertyName("latestVerdict")] string LatestVerdict,
    [property: JsonPropertyName("disposedReason")] string DisposedReason,
    [property: JsonPropertyName("lastHearingOutcome")] string LastHearingOutcome,
    [property: JsonPropertyName("custodyTimeLimit")] string? CustodyTimeLimit,
    [property: JsonPropertyName("latestPleaDescription")] string? LatestPleaDescription);

/// <summary>
/// Represents a charge. Currently an empty placeholder for future data structure.
/// </summary>
public record Charge(); // Empty list in JSON, expand if needed

/// <summary>
/// Represents a proposed charge against a defendant.
/// </summary>
public record ProposedCharge(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("caseId")] int? CaseId,
    [property: JsonPropertyName("defendantId")] int? DefendantId,
    [property: JsonPropertyName("surname")] string Surname,
    [property: JsonPropertyName("firstNames")] string FirstNames,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("location")] Location Location,
    [property: JsonPropertyName("fromDate")] string FromDate,
    [property: JsonPropertyName("toDate")] string ToDate,
    [property: JsonPropertyName("chargeParticulars")] string ChargeParticulars,
    [property: JsonPropertyName("anticipatedPlea")] string AnticipatedPlea,
    [property: JsonPropertyName("adjudicationCode")] string AdjudicationCode);

/// <summary>
/// Represents the location details associated with a charge.
/// </summary>
public record Location(
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("postcode")] string Postcode,
    [property: JsonPropertyName("addressLine1")] string AddressLine1,
    [property: JsonPropertyName("addressLine2")] string AddressLine2,
    [property: JsonPropertyName("addressLine3")] string AddressLine3,
    [property: JsonPropertyName("addressLine4")] string AddressLine4,
    [property: JsonPropertyName("addressLine5")] string AddressLine5,
    [property: JsonPropertyName("addressLine6")] string AddressLine6,
    [property: JsonPropertyName("addressLine7")] string AddressLine7,
    [property: JsonPropertyName("addressLine8")] string AddressLine8);

/// <summary>
/// Represents personal details of the defendant.
/// </summary>
public record PersonalDetail(
    [property: JsonPropertyName("address")] Address Address,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("ethnicity")] string Ethnicity,
    [property: JsonPropertyName("gender")] string Gender,
    [property: JsonPropertyName("occupation")] string Occupation,
    [property: JsonPropertyName("homePhoneNumber")] string HomePhoneNumber,
    [property: JsonPropertyName("mobilePhoneNumber")] string MobilePhoneNumber,
    [property: JsonPropertyName("workPhoneNumber")] string WorkPhoneNumber,
    [property: JsonPropertyName("preferredCorrespondenceLanguage")] string PreferredCorrespondenceLanguage,
    [property: JsonPropertyName("religion")] string Religion,
    [property: JsonPropertyName("guardian")] object Guardian);

/// <summary>
/// Represents an address structure.
/// </summary>
public record Address(
    [property: JsonPropertyName("postcode")] string Postcode,
    [property: JsonPropertyName("addressLine1")] string AddressLine1,
    [property: JsonPropertyName("addressLine2")] string AddressLine2,
    [property: JsonPropertyName("addressLine3")] string AddressLine3,
    [property: JsonPropertyName("addressLine4")] string AddressLine4,
    [property: JsonPropertyName("addressLine5")] string AddressLine5,
    [property: JsonPropertyName("addressLine6")] string AddressLine6,
    [property: JsonPropertyName("addressLine7")] string AddressLine7,
    [property: JsonPropertyName("addressLine8")] string AddressLine8);

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
