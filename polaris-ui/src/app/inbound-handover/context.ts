import { HandoverError } from "../common/errors/HandoverError";
import { validateUrn } from "../features/cases/logic/validate-urn";

// This is an opinionated first-pass of a context system whereby we are handed
//  over some context from another app.  The approach here is to nail down
//  a handed-over context into strongly typed variants.
// Feel free to chuck this approach away if ot doesn't hold as we add more
//  handover context situations.

type RecordObject = Record<string, any>;

type ContextIdentifiers = {
  caseId: number;
  urn: string | undefined;
  documentId: number | undefined;
};

type ContextHandoverObject = ContextIdentifiers & RecordObject;

const isValidContextHandoverObject = (o: any): o is ContextHandoverObject => {
  if (typeof o !== "object" || !Number.isInteger(o["caseId"])) {
    return false;
  }

  if (o["documentId"] !== undefined) {
    return Number.isInteger(o["caseId"]) && typeof o["urn"] === "string";
  }

  return true;
};

// Special case to accommodate a handover to go with e.g. caseId but no other context
type NakedContext = object;

const isNakedContext = (o: RecordObject): o is NakedContext =>
  Object.keys(o).length === 0;

// Triage task handover
type TriageContext = {
  taskId: number;
  taskTypeId: number;
};

const isTriageContext = (o: RecordObject): o is TriageContext =>
  Number.isInteger(o["taskId"]) && Number.isInteger(o["taskTypeId"]);

export type TaggedTriageContext = {
  contextType: "triage";
} & TriageContext;

export const isTaggedTriageContext = (
  o: RecordObject
): o is TaggedTriageContext => o.contextType === "triage" && isTriageContext(o);

//  Note: as more handover task/contexts are added, we could follow the pattern here as
//  shown by "AnExample" below.

// The data structure for our new context
type AnExampleContext = {
  someProperty: string;
};

// If we add this in to `queryStringToContextValues` then this function is a way to
//  discern the context object we are being passed is our new type
const isAnExampleContext = (o: RecordObject): o is AnExampleContext =>
  typeof o["someProperty"] === "string";

// Once we have isolated that the object is for a particular context, we tag it so
//  CWA code can tell which context we are in...
export type TaggedAnExampleContext = {
  contextType: "an-example";
} & AnExampleContext;

// ... by using this function to examine the context to see if it applies to whatever
//  specialised functionality is required to kick in for that context
export const isTaggedAnExampleContext = (
  o: RecordObject
): o is TaggedAnExampleContext =>
  o.contextType === "an-example" && isAnExampleContext(o);

// and add the new Tagged* type to TaggedContextObject
export type TaggedContext = TaggedTriageContext | TaggedAnExampleContext;

// So the public "api" (exported entities from this file) of your new context is
//  A type e.g. `TaggedAnExampleContext` that the consumer can use
//  A type guard e.g. `isTaggedAnExampleContext` that the consumer can use on
//   the context attribute in our `CombinedState` to see if there is a context
//   that is meaningful to specialised code.
// -----

export const isTaggedContext = (o: any): o is TaggedContext =>
  typeof o === "object" && "contextType" in o;

export const buildContextFromQueryString = (
  queryString: string
): {
  caseId: number;
  urn: string | undefined;
  documentId?: number | undefined;
  contextObject: TaggedContext | undefined;
  contextSearchParams: string;
} => {
  const ctxJson = new URLSearchParams(queryString).get("ctx");
  if (!ctxJson) {
    throw new HandoverError(
      `No ctx query parameter found when receiving an app handover: ${ctxJson}`
    );
  }

  let ctxWithCaseIdentifiers;
  try {
    ctxWithCaseIdentifiers = JSON.parse(ctxJson);
  } catch (ex) {
    throw new HandoverError(
      `Context object from handing-over app is not valid JSON: ${ctxJson}`
    );
  }

  if (!isValidContextHandoverObject(ctxWithCaseIdentifiers)) {
    throw new HandoverError(
      `Context object from handing-over app is missing identifiers: ${ctxJson}`
    );
  }

  const { caseId, urn, documentId, ...ctx } = ctxWithCaseIdentifiers;

  let rootUrn: string | undefined;
  if (urn) {
    const { isValid, rootUrn: validatedUrn } = validateUrn(urn);
    if (!isValid) {
      throw new HandoverError(`URN from handing-over app is not valid: ${urn}`);
    }
    rootUrn = validatedUrn;
  }

  const contextObjectAsRecord =
    ctx &&
    Object.keys(ctx).reduce((prev, curr) => {
      prev[curr] = String(ctx[curr]);
      return prev;
    }, {} as Record<string, string>);

  const searchParams = new URLSearchParams(contextObjectAsRecord);
  searchParams.sort();
  const searchParamsQuery = searchParams.toString();
  const contextSearchParams = searchParamsQuery ? `?${searchParamsQuery}` : "";

  let contextObject: TaggedContext | undefined;
  if (isTriageContext(ctx)) {
    contextObject = {
      contextType: "triage",
      ...ctx,
    };
  } else if (isAnExampleContext(ctx)) {
    contextObject = {
      contextType: "an-example",
      ...ctx,
    };
  } else if (isNakedContext(ctx)) {
    // not much point in this, for illustration purposes
    contextObject = undefined;
  } else {
    throw new HandoverError(
      `Context object from handing-over app is not of an expected form: ${ctxJson}`
    );
  }

  return {
    caseId,
    urn: rootUrn,
    documentId,
    contextObject,
    contextSearchParams,
  };
};
