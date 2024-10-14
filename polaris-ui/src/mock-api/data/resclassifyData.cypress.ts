import { MaterialType } from "../../app/features/cases/presentation/case-details/reclassify/data/MaterialType";
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
    typeId: 1019,
    description: "MG12",
    newClassificationVariant: "Exhibit",
  },
];
const dataSource = {
  materialTypeList,
};
export default dataSource;
