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
  let defaultValues: MappingDefaultData = {} as MappingDefaultData;

  const defaultArea = mappingData.areaMapping.find(
    (area) => area.ou === owningUnit
  );

  defaultValues.cpsArea = defaultArea?.areaId ?? "";
  defaultValues.businessUnit = defaultArea?.unitId ?? "";

  const defaultDocType = mappingData.docTypeMapping.find(
    (docType) => docType.cmsDocTypeId === `${docTypeId}`
  );

  defaultValues.documentType = defaultDocType?.docTypeId ?? "";

  const defaultIA = mappingData.iAMapping.find((ia) => ia.ou === urnSubString);
  const defaultIAFromOuCodeMapping =
    ouCodeMapping.find((mapping) => mapping.ouCode === urnSubString.slice(0, 2))
      ?.investigatingAgencyCode ?? "";

  defaultValues.investigatingAgency =
    defaultIA?.ia ?? defaultIAFromOuCodeMapping;

  return defaultValues;
};
