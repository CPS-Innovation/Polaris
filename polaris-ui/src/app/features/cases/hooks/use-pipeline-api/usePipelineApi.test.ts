import { usePipelineApi } from "./usePipelineApi";
import * as polling from "./initiate-and-poll";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { renderHook } from "@testing-library/react-hooks";
import { AsyncPipelineResult } from "./AsyncPipelineResult";

// describe("usePipelineApi", () => {
//   it("can return results", async () => {
//     const expectedResults = {
//       pipelineBusy: true,
//       pipelineResults: {} as AsyncPipelineResult<PipelineResults>,
//     };

//     jest
//       .spyOn(polling, "initiateAndPoll")
//       .mockImplementation(
//         (
//           urn,
//           caseId,
//           pollingDelay,
//           lastProcessingCompleted,
//           correlationId,
//           del
//         ) => {
//           new Promise((resolve) => setTimeout(resolve, 50)).then(() =>
//             del(expectedResults.pipelineResults)
//           );
//           return () => {};
//         }
//       );

//     const { result, waitForNextUpdate } = renderHook(() =>
//       usePipelineApi(
//         "0",
//         1,
//         {
//           startPipelineRefresh: true,
//           lastProcessingCompleted: "",
//         },
//         () => false
//       )
//     );

//     expect(result.current).toEqual({
//       pipelineBusy: true,
//       pipelineResults: {
//         status: "initiating",
//         haveData: false,
//         correlationId: "",
//       },
//     });

//     await waitForNextUpdate();

//     expect(result.current).toEqual(expectedResults);
//   });
// });
