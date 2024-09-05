import { MaterialType } from "../../app/features/cases/presentation/case-details/reclassify/data/MaterialType";
import { ExhibitProducer } from "../../app/features/cases/presentation/case-details/reclassify/data/ExhibitProducer";
import { StatementWitness } from "../../app/features/cases/presentation/case-details/reclassify/data/StatementWitness";
import { StatementWitnessNumber } from "../../app/features/cases/presentation/case-details/reclassify/data/StatementWitnessNumber";
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
  {
    typeId: 6,
    description: "MG6",
    newClassificationVariant: "Immediate",
  },
];

const exhibitProducers: ExhibitProducer[] = [
  { id: 1, exhibitProducer: "PC Blaynee" },
  { id: 2, exhibitProducer: "PC Jones" },
  { id: 3, exhibitProducer: "PC Lucy" },
];

const statementWitness: StatementWitness[] = [
  { witness: { id: 1, name: "PC Blaynee_S" } },
  { witness: { id: 2, name: "PC Jones_S" } },
  { witness: { id: 3, name: "PC Lucy_S" } },
];

const statementWitnessNumbers: StatementWitnessNumber[] = [
  {
    documentId: 1,
    statementNumber: 2,
  },
  {
    documentId: 2,
    statementNumber: 3,
  },
  {
    documentId: 3,
    statementNumber: 4,
  },
  {
    documentId: 4,
    statementNumber: 7,
  },
];
const dataSource = {
  materialTypeList,
  exhibitProducers,
  statementWitness,
  statementWitnessNumbers,
};
export default dataSource;
