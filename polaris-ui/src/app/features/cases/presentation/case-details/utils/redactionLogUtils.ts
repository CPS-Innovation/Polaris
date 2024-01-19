import {
  RedactionLogMappingData,
  OuCodeMapping,
} from "../../../domain/redactionLog/RedactionLogData";
import { UnderRedactionFormData } from "../../../domain/redactionLog/RedactionLogFormData";

type MappingDefaultData = Omit<
  UnderRedactionFormData,
  "notes" | "chargeStatus"
>;
export const getDefaultValuesFromMappings = (
  mappingData: RedactionLogMappingData,
  ouCodeMapping: OuCodeMapping[],
  owningUnit: string,
  docTypeId: number,
  caseUrn: string
) => {
  let defaultValues: MappingDefaultData = {
    cpsArea: "",
    businessUnit: "",
    documentType: "",
    investigatingAgency: "",
  };

  const defaultArea = mappingData.businessUnits.find(
    (area) => area.ou === owningUnit
  );

  defaultValues.cpsArea = defaultArea?.areaId ?? "";
  defaultValues.businessUnit = defaultArea?.unitId ?? "";

  const defaultDocType = mappingData.documentTypes.find(
    (docType) => docType.cmsDocTypeId === `${docTypeId}`
  );

  defaultValues.documentType = defaultDocType?.docTypeId ?? "";

  const defaultIA = mappingData.investigatingAgencies.find(
    (ia) => ia.ouCode === caseUrn.slice(0, 4)
  );
  const defaultIAFromOuCodeMapping =
    ouCodeMapping.find((mapping) => mapping.ouCode === caseUrn.slice(0, 2))
      ?.investigatingAgencyCode ?? "";

  defaultValues.investigatingAgency =
    defaultIA?.investigatingAgencyId ?? defaultIAFromOuCodeMapping;

  return defaultValues;
};

export const redactString = (
  str: string,
  trimFrontCount: number = 1,
  trimBackCount: number = 1
) => {
  let chars = str.split("");
  for (let i = trimFrontCount; i < chars.length - trimBackCount; i++) {
    chars[i] = "*";
  }
  return chars.join("");
};

export const getPresentationRedactionTypeNames = (
  count: number,
  name: string
) => {
  if (count < 2) {
    if (name === "Previous convictions") {
      return "Previous conviction";
    }
    return name;
  }
  switch (name) {
    case "Bank details":
    case "Previous convictions":
      return name;
    case "Address":
    case "Email address":
      return `${name}es`;
    case "Date of birth":
      return "Dates of birth";
    case "Relationship to others":
      return "Relationships to others";
    default:
      return `${name}s`;
  }
};
