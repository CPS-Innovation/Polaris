export class CmsAuthError extends Error {
  public readonly name: string = "CMS_AUTH_ERROR";
  public readonly customMessage?: string;
  constructor(message: string, customMessage?: string) {
    super(message);
    this.customMessage = customMessage;
  }
}
