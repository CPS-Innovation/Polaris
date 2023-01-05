export type ApiResult<T> =
  | {
      status: "loading";
    }
  | {
      error: any;
      status: "failed";
      httpStatusCode: number | undefined;
    }
  | {
      status: "succeeded";
      data: T;
    };
