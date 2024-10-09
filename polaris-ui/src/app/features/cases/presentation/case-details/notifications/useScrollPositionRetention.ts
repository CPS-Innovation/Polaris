import { useEffect, useRef, useState } from "react";

export const useScrollPositionRetention = <T extends HTMLElement>(
  isElOpen: boolean
) => {
  const elRef = useRef<T | null>(null);
  const [scrollPosition, setScrollPosition] = useState<number>();

  const handleScroll: React.UIEventHandler<T> = (event) => {
    setScrollPosition((event?.target as HTMLElement)?.scrollTop);
  };

  useEffect(() => {
    if (isElOpen && elRef.current && scrollPosition !== undefined) {
      elRef.current.scrollTo({ top: scrollPosition });
    }
  }, [isElOpen, scrollPosition]);

  return [elRef, handleScroll] as const;
};
