export interface ListItem {
  id: string;
  name: string;
  children: ListItem[];
}

export type ListItemWithoutChildren = Omit<ListItem, "children">;
