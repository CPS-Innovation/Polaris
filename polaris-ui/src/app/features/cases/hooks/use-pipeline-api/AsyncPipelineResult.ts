// If something is, say, polling, we need a shape to indicate that
//  a process can go back to loading, but still retains its previous data
export type AsyncPipelineResult<T> =
  | {
      status: "initiating";
      data?: T;
      correlationId: string;
    }
  | {
      status: "incomplete";
      data: T;
      correlationId: string;
    }
  | {
      status: "complete";
      data: T;
      correlationId: string;
    }
  | {
      status: "failed";
      error: any;
      data?: T;
      httpStatusCode: number | undefined;
      correlationId: string;
    };
