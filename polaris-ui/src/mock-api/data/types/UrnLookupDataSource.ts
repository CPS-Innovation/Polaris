import { UrnLookupResult } from "../../../app/features/cases/domain/gateway/UrnLookupResult";

export type UrnLookupDataSource = (caseId: number) => UrnLookupResult;
