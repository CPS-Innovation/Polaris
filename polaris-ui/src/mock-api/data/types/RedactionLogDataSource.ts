import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
} from "../../../app/features/cases/domain/redactionLog/RedactionLogData";

export type RedactionLogDataSource = {
  lookUpsData: RedactionLogLookUpsData;
  mappingData: RedactionLogMappingData;
};
