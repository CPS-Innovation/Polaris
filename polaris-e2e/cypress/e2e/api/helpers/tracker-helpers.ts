import { PipelineResults } from "../../../../gateway/PipelineResults"

export const isTrackerReady = ({
  status,
  body,
}: ApiResponseBody<PipelineResults>) => {
  if (status != 200) {
    return false
  }
  const pipelineStatus = body?.status
  if (pipelineStatus == "Failed") {
    throw new Error("Pipeline failed, ending test")
  }
  return pipelineStatus == "Completed"
}
