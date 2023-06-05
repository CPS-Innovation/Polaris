export class ApiError extends Error {
  public readonly name: string = "API_ERROR";
  public readonly path: string;
  public readonly code: number;
  public readonly customProperties?: Record<string, string>;
  constructor(
    message: string,
    path: string,
    { status, statusText }: { status: number; statusText: string },
    customProperties?: Record<string, string>
  ) {
    super(
      `An error ocurred contacting the server at ${path}: ${message}; status - ${statusText} (${status})`
    );
    this.path = path;
    this.code = status;
    this.customProperties = customProperties;
  }
}
