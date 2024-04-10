import { Note } from "../../../app/features/cases/domain/gateway/NotesData";

export type NotesDataSource = (documentId: string) => Note[];
