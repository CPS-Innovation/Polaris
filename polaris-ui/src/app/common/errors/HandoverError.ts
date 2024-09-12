export class HandoverError extends Error {
  public readonly name: string = "HANDOVER_ERROR";
  public readonly customMessage?: string;

  constructor(message: string, customMessage?: string) {
    super(message);
    this.customMessage = customMessage;
  }
}
