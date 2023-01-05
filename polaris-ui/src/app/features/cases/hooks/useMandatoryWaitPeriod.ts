import { useEffect, useRef, useState } from "react";

type TimerRef = ReturnType<typeof setTimeout>;

type LogEvent = "externalReady" | "enterMandatoryWait" | "exitMandatoryWait";
type PublicState = "preWait" | "wait" | "ready";

export const useMandatoryWaitPeriod = (
  isExternalReady: boolean,
  preWaitPeriodMs: number,
  mandatoryWaitPeriodMs: number
): PublicState => {
  const [eventLog, setEventLog] = useState<LogEvent[]>([]);

  const timer1Ref = useRef<TimerRef>();
  const timer2Ref = useRef<TimerRef>();

  useEffect(() => {
    if (isExternalReady) {
      setEventLog((log) => [...log, "externalReady"]);
    } else {
      // we are either initialising, or have gone back to
      //  isExternalReady === false, so set the log to empty and clear
      //  any timeouts that were previosuly set up.
      setEventLog([]);
      clearTimeout(timer1Ref.current);
      clearTimeout(timer2Ref.current);

      timer1Ref.current = setTimeout(() => {
        // when enterMandatoryWait enters the log, we are in our mandatory wait period
        setEventLog((log) => [...log, "enterMandatoryWait"]);
      }, preWaitPeriodMs);

      timer2Ref.current = setTimeout(() => {
        // when exitMandatoryWait enters the log, we have exited the madatory wait period
        setEventLog((log) => [...log, "exitMandatoryWait"]);
      }, preWaitPeriodMs + mandatoryWaitPeriodMs);
    }
  }, [isExternalReady, preWaitPeriodMs, mandatoryWaitPeriodMs]);

  if (!eventLog.length) {
    // Our log is [], so the external flag is not ready, and nothing else
    //  has happened, so we are in the pre-wait period
    return preWaitPeriodMs ? "preWait" : "wait";
  }

  if (
    // If externalReady happened before timer1 i.e. log[0] === "externalReady)",
    // we beat the preWaitPeriodMs period no need for mandatory wait.
    eventLog[0] === "externalReady" ||
    // Otherwise, if all three events have happened we are ready.
    //  i.e. ["enterMandatoryWait", "externalReady", "exitMandatoryWait"] means the external flag was ready
    //   during the mandatory wait period, but then timer2 finished the wait period
    //  i.e. ["enterMandatoryWait", "exitMandatoryWait" "externalReady"] means timer2 finished and we were
    //   still waiting for the external flag to be ready, which it did eventually
    eventLog.length === 3
  ) {
    return "ready";
  }
  // all other eventualities mean we are in the wait period.
  return "wait";
};
