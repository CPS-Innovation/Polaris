export type CaseDetails = {
  id: number;
  uniqueReferenceNumber: string;
  isCaseCharged: boolean;
  owningUnit: string;
  numberOfDefendants: number;
  leadDefendantDetails: DefendantDetails | null;
  headlineCharge: HeadlineCharge;
  defendants: Defendant[];
  witnesses: Witness[];
};

export type Defendant = {
  defendantDetails: DefendantDetails;
  custodyTimeLimit: CustodyTimeLimit;
  charges: Charge[];
};

export type Witness = {
  id: number;
  shoulderNumber: string | null;
  title: string;
  name: string;
  hasStatements: boolean;
  listOrder: number | null;
  child: boolean;
  expert: boolean;
  greatestNeed: boolean;
  prisoner: boolean;
  interpreter: boolean;
  vulnerable: boolean;
  police: boolean;
  professional: boolean;
  specialNeeds: boolean;
  intimidated: boolean;
  victim: boolean;
};

export type DefendantDetails = {
  id: number;
  listOrder: number;
  firstNames: string;
  surname: string;
  organisationName: string;
  dob: string;
  youth: boolean;
  type: string;
};

type HeadlineCharge = {
  charge: string;
  date: string;
  nextHearingDate: string;
};

type Charge = {
  id: number;
  listOrder: number;
  isCharged: boolean;
  nextHearingDate: string;
  earlyDate: string;
  lateDate: string;
  code: string;
  shortDescription: string;
  longDescription: string;
  custodyTimeLimit: CustodyTimeLimit;
};

export type CustodyTimeLimit = {
  expiryDate: string;
  expiryDays: number;
  expiryIndicator: "ACTIVE" | "EXPIRED" | null;
};
