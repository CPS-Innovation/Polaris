type Props = {
  noTopMargin?: boolean;
};

export const SectionBreak: React.FC<Props> = ({ noTopMargin }) => (
  <hr
    className="govuk-section-break govuk-section-break--m govuk-section-break--visible"
    style={noTopMargin ? { marginTop: 0 } : {}}
  />
);
