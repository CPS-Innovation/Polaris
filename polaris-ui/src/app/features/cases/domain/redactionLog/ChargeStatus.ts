export enum ChargeStatus {
  PreCharge = 1,
  PostCharge,
}
export const ChargeStatusLabels = {
  [ChargeStatus.PreCharge]: "Pre-charge",
  [ChargeStatus.PostCharge]: "Post-charge",
};
