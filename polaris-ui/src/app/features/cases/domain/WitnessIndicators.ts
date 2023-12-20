import { Witness } from "./gateway/CaseDetails";

export type WitnessIndicator = keyof typeof witnessIndicatorNames;

export const witnessIndicatorNames = {
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

export const witnessIndicatorPrecedenceOrder: WitnessIndicator[] = [
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
