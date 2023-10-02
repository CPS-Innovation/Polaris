import React, { useEffect, useState } from "react";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { Select } from "../../../../../common/presentation/components";
import classes from "./RedactButton.module.scss";

type Props = {
  onConfirm: () => void;
};

export const RedactButton: React.FC<Props> = ({ onConfirm }) => {
  const [redactionType, setRedactionType] = useState<string | null>(null);
  useFocusTrap("#redact-modal");
  useEffect(() => {
    console.log("helloooo");
  }, []);

  const items = [
    {
      children: "Option 1",
      value: "1" as const,
    },
    {
      children: "Option 2",
      value: "2" as const,
    },
  ];
  return (
    <div id="redact-modal" className={classes.redactionModal}>
      <Select
        data-testid="select-result-order"
        value={"2"}
        items={items}
        formGroup={{
          className: classes.select,
        }}
        onChange={(ev) => {
          setRedactionType(ev.target.value);
          console.log("ev.target.value", ev.target.value);
        }}
      />
      <button
        className={classes.button}
        disabled={!redactionType}
        onClick={onConfirm}
        data-testid="btn-redact"
        id="btn-redact"
      >
        Redact
      </button>

      {/* <button className={classes.button} onClick={onConfirm}>
        Redact-0
      </button> */}
    </div>
  );
};
