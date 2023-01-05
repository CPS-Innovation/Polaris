import { ApiTextSearchResult } from "../../../app/features/cases/domain/ApiTextSearchResult";

export type SearchCaseDataSource = (query: string) => ApiTextSearchResult[];
