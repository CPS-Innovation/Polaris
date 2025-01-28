export class ApiError extends Error {
  public readonly name: string = "API_ERROR";
  public readonly customMessage?: string;

  constructor(message: string, customMessage?: string) {
    super(message);
    this.customMessage = customMessage;
  }
}
