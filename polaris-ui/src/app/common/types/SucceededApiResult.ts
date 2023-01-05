export type SucceededApiResult<T> = {
  status: "succeeded";
  data: T;
};
