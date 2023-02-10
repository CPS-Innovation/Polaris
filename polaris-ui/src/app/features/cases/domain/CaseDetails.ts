export type CaseDetails = {
  id: number;
  uniqueReferenceNumber: string;
  isCaseCharged: boolean;
  numberOfDefendants: number;
  leadDefendantDetails: DefendantDetails;
  headlineCharge: HeadlineCharge;
  defendants: {
    defendantDetails: DefendantDetails;
    custodyTimeLimit: CustodyTimeLimit;
    charges: Charge[];
  }[];
};

type DefendantDetails = {
  id: number;
  listOrder: number;
  firstNames: string;
  surname: string;
  organisationName: string;
  dob: string;
  isYouth: boolean;
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
  expiryIndicator: ExpiryIndicator | null;
};

export enum ExpiryIndicator {
  active = "Active",
  expired = "Expired",
}
