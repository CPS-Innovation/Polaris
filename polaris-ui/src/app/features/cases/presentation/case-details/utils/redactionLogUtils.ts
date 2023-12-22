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

  defaultValues.cpsArea = defaultArea?.areaId ?? "";
  defaultValues.businessUnit = defaultArea?.unitId ?? "";

  const defaultDocType = mappingData.documentTypes.find(
    (docType) => docType.cmsDocTypeId === `${docTypeId}`
  );

  defaultValues.documentType = defaultDocType?.docTypeId ?? "";

  const defaultIA = mappingData.investigatingAgencies.find(
    (ia) => ia.ouCode === urnSubString
  );
  const defaultIAFromOuCodeMapping =
    ouCodeMapping.find((mapping) => mapping.ouCode === urnSubString.slice(0, 2))
      ?.investigatingAgencyCode ?? "";

  defaultValues.investigatingAgency =
    defaultIA?.investigatingAgencyId ?? defaultIAFromOuCodeMapping;

  console.log("defaultValues>>", defaultValues);

  return defaultValues;
};
