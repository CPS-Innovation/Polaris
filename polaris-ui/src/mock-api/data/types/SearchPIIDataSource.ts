import { SearchPIIResultItem } from "../../../app/features/cases/domain/gateway/SearchPIIData";

export type SearchPIIDataSource = (documentId: string) => SearchPIIResultItem[];
