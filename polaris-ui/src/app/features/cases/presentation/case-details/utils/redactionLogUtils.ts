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
  urnSubString: string
) => {
  console.log("urnSubString>>", urnSubString);
  console.log("docTypeId>>", docTypeId);
  let defaultValues: MappingDefaultData = {
    cpsArea: "",
    businessUnit: "",
    documentType: "",
    investigatingAgency: "",
  };

  const defaultArea = mappingData.businessUnits.find(
    (area) => area.ou === owningUnit
  );

  console.log("defaultArea>>>", defaultArea);

  if (defaultArea && defaultArea.areaId !== "null") {
    defaultValues.cpsArea = defaultArea.areaId;
  }
  if (defaultArea && defaultArea.unitId !== "null") {
    defaultValues.cpsArea = defaultArea.unitId;
  }

  const defaultDocType = mappingData.documentTypes.find(
    (docType) => docType.cmdDocTypeId === `${docTypeId}`
  );

  defaultValues.documentType = defaultDocType?.docTypeId ?? "";

  const defaultIA = mappingData.investigatingAgencies.find(
    (ia) => ia.ouCode === urnSubString
  );
  const defaultIAFromOuCodeMapping =
    ouCodeMapping.find((mapping) => mapping.ouCode === urnSubString.slice(0, 2))
      ?.investigatingAgencyCode ?? "";

  console.log("defaultIA>>", defaultIA);
  if (defaultIA && defaultIA?.investigatingAgencyId !== "null") {
    defaultValues.investigatingAgency = defaultIA.investigatingAgencyId;
  }
  if (
    defaultIA?.investigatingAgencyId === "null" &&
    defaultIAFromOuCodeMapping
  ) {
    defaultValues.investigatingAgency = defaultIAFromOuCodeMapping;
  }

  return defaultValues;
};
