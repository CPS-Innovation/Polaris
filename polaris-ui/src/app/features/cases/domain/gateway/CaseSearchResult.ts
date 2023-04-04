import { CaseDetails } from "./CaseDetails";

// At the time of writing the gateway returns the exact same model for
//  the search as the get-case endpoint. These might diverge eventually,
//  so lets at least give search results their own type name, but base it
//  off the CaseDetails model.

export type CaseSearchResult = Omit<CaseDetails, "defendants">;
