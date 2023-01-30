export class CmsAuthError extends Error {
  public readonly name: string = "CMS_AUTH_ERROR";

  constructor(message: string) {
    super(message);
  }
}
