export class CmsError extends Error {
  public readonly name: string = "CMS_ERROR";

  constructor(message: string) {
    super(message);
  }
}
