import { NotesData } from "../../app/features/cases/domain/gateway/NotesData";
import { NotesDataSource } from "./types/NotesDataSource";

export const notes: NotesData[] = [
  {
    documentId: "1",
    notes: [
      {
        id: 1,
        createdByName: "rrr",
        sortOrder: 1,
        date: "abc",
        text: "note text",
        type: "abc",
      },
    ],
  },
  {
    documentId: "2",
    notes: [
      {
        id: 1,
        createdByName: "rrr_2",
        sortOrder: 1,
        date: "abc",
        text: "note text_2",
        type: "abc",
      },
    ],
  },
];

const dataSource: NotesDataSource = (documentId: string) => {
  const notesData = notes.find((note) => note.documentId === documentId);
  if (notesData) {
    return notesData.notes;
  }
  return [];
};

export default dataSource;
