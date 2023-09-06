type PanelProps = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLDivElement>,
  HTMLDivElement
>;

export type ItemProps = {
  id: string;
  label: string;
  panel: PanelProps;
  isDirty: boolean;
};

export type CommonTabsProps = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> & {
  idPrefix: string;
  title: string;
  items: ItemProps[];
};
