import { CaseSearchResult } from "../../../app/features/cases/domain/CaseSearchResult";

export type SearchDataSource = (urn: string) => CaseSearchResult[];
