import { NotesDataSource } from "./types/NotesDataSource";

export const notes = [
  {
    documentId: "1",
    notes: [
      {
        id: 1,
        createdByName: "test_user1",
        sortOrder: 1,
        date: "2024-02-10",
        text: "text 1",
        type: "abc",
      },
    ],
  },
  {
    documentId: "2",
    notes: [
      {
        id: 1,
        createdByName: "test_user1",
        sortOrder: 1,
        date: "2024-02-10",
        text: "text_1",
        type: "abc",
      },
      {
        id: 1,
        createdByName: "test_user2",
        sortOrder: 2,
        date: "2024-02-11",
        text: "text_2",
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
