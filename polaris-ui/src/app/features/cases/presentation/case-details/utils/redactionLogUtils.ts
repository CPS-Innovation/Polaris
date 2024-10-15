import { CmsDocType } from "../../../domain/gateway/CmsDocType";
import {
  RedactionLogMappingData,
  OuCodeMapping,
} from "../../../domain/redactionLog/RedactionLogData";
import { UnderRedactionFormData } from "../../../domain/redactionLog/RedactionLogFormData";

type MappingDefaultData = Omit<
  UnderRedactionFormData,
  "notes" | "chargeStatus"
>;

const PNC_PRINT_DOCUMENT_TYPE_ID = "34";
export const getDefaultValuesFromMappings = (
  mappingData: RedactionLogMappingData,
  ouCodeMapping: OuCodeMapping[],
  owningUnit: string,
  docTypeId: CmsDocType["documentTypeId"],
  caseUrn: string
) => {
  const manuallySelectDocumentTypeIds = [-1, 1029, 1200];
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
  if (
    docTypeId !== null &&
    !manuallySelectDocumentTypeIds.includes(docTypeId) &&
    defaultDocType?.docTypeId
  ) {
    defaultValues.documentType = defaultDocType.docTypeId;
    if (docTypeId === 1056 || docTypeId === 1057) {
      defaultValues.documentType = PNC_PRINT_DOCUMENT_TYPE_ID;
    }
  }

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

export const removeNonDigits = (input: string) => {
  return input.replace(/\D/g, "");
};
