export type SaveStatus =
  | {
      type: "redaction" | "rotation";
      status: "saving" | "saved" | "error";
    }
  | { type: "none"; status: "initial" };
