import { NotesData } from "../../app/features/cases/domain/gateway/NotesData";
import { NotesDataSource } from "./types/NotesDataSource";

export const notes = [
  {
    documentId: "1",
    notes: [
      {
        id: 1,
        createdByName: "rrr",
        sortOrder: 1,
        date: "2024-02-10",
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
        date: "2024-02-10",
        text: "note text_2",
        type: "abc",
      },
      {
        id: 1,
        createdByName: "rrr_2",
        sortOrder: 2,
        date: "2024-02-10",
        text: "note text_3",
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
