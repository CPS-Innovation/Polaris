export type Note = {
  id: number;
  createdByName: string;
  sortOrder: number;
  date: string;
  text: string;
  type: string;
};

export type NotesData = {
  documentId: string;
  notes: Note[];
  addNoteStatus: "failure" | "saving" | "success" | "initial";
  getNoteStatus: "initial" | "loading";
};
