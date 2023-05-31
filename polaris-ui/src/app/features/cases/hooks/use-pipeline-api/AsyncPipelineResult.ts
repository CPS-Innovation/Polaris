// If something is, say, polling, we need a shape to indicate that
//  a process can go back to loading, but still retains its previous data
export type AsyncPipelineResult<T> =
  | {
      status: "initiating";
      haveData: false;
      correlationId: string;
    }
  | {
      status: "incomplete";
      haveData: true;
      data: T;
      correlationId: string;
    }
  | {
      status: "complete";
      haveData: true;
      data: T;
      correlationId: string;
    }
  | {
      status: "failed";
      error: any;
      httpStatusCode: number | undefined;
      haveData: false;
      correlationId: string;
    };
