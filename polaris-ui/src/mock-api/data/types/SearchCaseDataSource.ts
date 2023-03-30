import { ApiTextSearchResult } from "../../../app/features/cases/domain/gateway/ApiTextSearchResult";

export type SearchCaseDataSource = (query: string) => ApiTextSearchResult[];
