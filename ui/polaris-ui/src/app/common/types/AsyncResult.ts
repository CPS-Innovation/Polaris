export type AsyncResult<T> =
  | {
      status: "loading";
    }
  | {
      status: "succeeded";
      data: T;
    };
