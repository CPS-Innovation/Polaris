import { Witness } from "../../../../domain/gateway/CaseDetails";

export type StatementWitness = {
  witness: Witness;
  statementNumbers: number[];
};
