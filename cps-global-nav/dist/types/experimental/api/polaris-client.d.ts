import { UrnLookupResult } from "../domain/UrnLookupResult";
import { CaseDetails } from "../domain/CaseDetails";
export declare const lookupUrn: (caseId: number) => Promise<UrnLookupResult>;
export declare const getCaseDetails: (urn: string, caseId: number) => Promise<CaseDetails>;
