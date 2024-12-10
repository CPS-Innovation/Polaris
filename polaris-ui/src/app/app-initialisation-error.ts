// This global object exists only as a channel to communicate from pre-app-rendering code that kicks off in the
//  api reauthentication space to the Error boundary.  If an error is identified before app load then
//  we attach it here and the ErrorBoundary will check this object on initialisation.

const appInitialisationError: {
  error?: Error;
} = {};

export default appInitialisationError;
