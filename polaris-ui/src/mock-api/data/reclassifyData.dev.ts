import { MaterialType } from "../../app/features/cases/presentation/case-details/reclassify/data/MaterialType";
import { ExhibitProducer } from "../../app/features/cases/presentation/case-details/reclassify/data/ExhibitProducer";
import { StatementWitness } from "../../app/features/cases/presentation/case-details/reclassify/data/StatementWitness";
export const materialTypeList: MaterialType[] = [
  {
    typeId: 1015,
    description: "MG10",
    newClassificationVariant: "Immediate",
  },
  {
    typeId: 1031,
    description: "MG11",
    newClassificationVariant: "Statement",
  },
  {
    typeId: 1042,
    description: "MG15(SDN)",
    newClassificationVariant: "Exhibit",
  },
  {
    typeId: 1029,
    description: "Other Communication",
    newClassificationVariant: "Other",
  },
];

const exhibitProducers: ExhibitProducer[] = [
  { id: 1, exhibitProducer: "PC Blaynee" },
  { id: 2, exhibitProducer: "PC Jones" },
  { id: 3, exhibitProducer: "PC Lucy" },
];

const statementWitness: StatementWitness[] = [
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
