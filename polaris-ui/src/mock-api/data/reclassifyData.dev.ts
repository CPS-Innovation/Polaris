const materialTypeList = [
  {
    code: "1015",
    description: "MG10",
    longDescription: "Witness Non-Availability",
    classification: "OTHER",
    isCaseMaterial: "IN-Y",
    listOrder: 280,
    addAsUsedOrUnused: "Y",
    sectionWhenCreated: "MG Forms",
  },
  {
    code: "1031",
    description: "MG11",
    longDescription: "Witness Statement",
    classification: "STATEMENT",
    isCaseMaterial: "IN-Y",
    listOrder: 290,
    addAsUsedOrUnused: "Y",
    sectionWhenCreated: "Witnesses",
  },
  {
    code: "1042",
    description: "MG15(SDN)",
    longDescription: "Short Descriptive Note",
    classification: "EXHIBIT",
    isCaseMaterial: "IN-Y",
    listOrder: 340,
    addAsUsedOrUnused: "Y",
    sectionWhenCreated: "Exhibits",
  },
  {
    code: "1029",
    description: "Other Communication",
    longDescription: "Other Communication",
    classification: "OTHER",
    isCaseMaterial: "N",
    canBeAdded: "N",
    listOrder: 500,
    addAsUsedOrUnused: "N",
    isCommunicationItem: "Y",
  },
];

const exhibitProducers = [
  { id: 1, fullName: "PC Blaynee" },
  { id: 2, fullName: "PC Jones" },
  { id: 3, fullName: "PC Lucy" },
];

const statementWitness = [
  { witness: { id: 1, name: "PC Blaynee_S" }, statementNumbers: [1, 2, 3] },
  { witness: { id: 2, name: "PC Jones_S" }, statementNumbers: [2, 3] },
  { witness: { id: 3, name: "PC Lucy_S" }, statementNumbers: [2, 3, 4] },
];
const dataSource = {
  materialTypeList,
  exhibitProducers,
  statementWitness,
};
export default dataSource;
