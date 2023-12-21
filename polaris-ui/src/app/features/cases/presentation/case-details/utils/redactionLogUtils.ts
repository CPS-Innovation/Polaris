import { RedactionLogMappingData } from "../../../domain/redactionLog/RedactionLogData";
import { UnderRedactionFormData } from "../../../domain/redactionLog/RedactionLogFormData";

type MappingDefaultData = Omit<
  UnderRedactionFormData,
  "notes" | "chargeStatus"
>;
export const getDefaultValuesFromMappings = (
  mappingData: RedactionLogMappingData,
  owningUnit: string,
  docTypeId: string,
  urnSubString: string
) => {
  let defaultValues: MappingDefaultData = {} as MappingDefaultData;

  const defaultArea = mappingData.areaMapping.find(
    (area) => area.ou === owningUnit
  );

  defaultValues.cpsArea = defaultArea?.areaId ?? "";
  defaultValues.businessUnit = defaultArea?.unitId ?? "";

  const defaultDocType = mappingData.docTypeMapping.find(
    (docType) => docType.cmdDocTypeId === docTypeId
  );

  defaultValues.documentType = defaultDocType?.docTypeId ?? "";

  const defaultIA = mappingData.iAMapping.find((ia) => ia.ou === urnSubString);

  defaultValues.investigatingAgency = defaultIA?.ia ?? "";

  return defaultValues;
};
