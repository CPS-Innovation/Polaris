import { Witness } from "./gateway/CaseDetails";

export type WitnessIndicator = keyof typeof WitnessIndicatorNames;

export const WitnessIndicatorNames = {
  V: "Victim",
  P: "Police",
  C: "Child",
  F: "Professional",
  X: "Expert",
  L: "Vulnerable",
  T: "Intimidated",
  G: "Greatest Needs",
  S: "Special",
  H: "Prisoner",
  I: "Interpreter",
};

export const WitnessIndicatorPrecedenceOrder: WitnessIndicator[] = [
  "V",
  "P",
  "C",
  "F",
  "X",
  "L",
  "T",
  "G",
  "S",
  "H",
  "I",
];

export const witnessIndicatorLetters: Partial<
  Record<keyof Witness, WitnessIndicator>
> = {
  victim: "V",
  police: "P",
  child: "C",
  professional: "F",
  expert: "X",
  vulnerable: "L",
  intimidated: "T",
  greatestNeed: "G",
  specialNeeds: "S",
  prisoner: "H",
  interpreter: "I",
};
