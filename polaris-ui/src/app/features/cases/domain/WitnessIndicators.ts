export type WitnessIndicator = keyof typeof WitnessIndicatorNames;

export const WitnessIndicatorNames = {
  G: "Full text",
  H: "es sef  dssadasdadda",
  I: "foo",
};

export const WitnessIndicatorPrecedenceOrder: WitnessIndicator[] = [
  "G",
  "I",
  "H",
];
