import { CaseSearchResult } from "../../../app/features/cases/domain/gateway/CaseSearchResult";

export type SearchDataSource = (urn: string) => CaseSearchResult[];
