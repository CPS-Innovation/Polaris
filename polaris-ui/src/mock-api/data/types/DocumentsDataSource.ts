import { CaseDocument } from "../../../app/features/cases/domain/CaseDocument";

export type DocumentsDataSource = (id: number) => CaseDocument[];
