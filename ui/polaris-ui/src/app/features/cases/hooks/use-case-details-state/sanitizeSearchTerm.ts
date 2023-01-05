export const sanitizeSearchTerm = (searchTerm: string) =>
  searchTerm.trim().split(" ")[0];
