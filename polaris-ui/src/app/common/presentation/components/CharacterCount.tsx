import React, { useEffect, useState, useCallback } from "react";
import { TextArea } from "./TextArea";
import classes from "./CharacterCount.module.scss";
type CharacterCountProps = {
  name: string;
  id: string;
  label: {
    children: React.ReactNode;
  };
  value: string;
  hint?: {
    children: React.ReactNode;
  };
  maxCharacters: number;
  errorMessage?: {
    children: React.ReactNode;
  };
  onChange: (value: React.ChangeEvent<HTMLTextAreaElement>) => void;
};
export const CharacterCount: React.FC<CharacterCountProps> = (props) => {
  const [characterCount, setCharacterCount] = useState(props.maxCharacter);
  useEffect(() => {
    const timer = setTimeout(() => {
      setCharacterCount(props.maxCharacter - props.value.length);
    }, 500);
    return () => {
      clearTimeout(timer);
    };
  });

  const getCharacterRemainingText = useCallback((count: number) => {
    const absCount = Math.abs(count);
    if (count >= 0) {
      return count === 1
        ? `You have ${absCount} character remaining`
        : `You have ${absCount} characters remaining`;
    }
    return count === -1
      ? `You have ${absCount} character too many`
      : `You have ${absCount} characters too many`;
  }, []);
  return (
    <div className={classes.characterCount}>
      <TextArea {...props} />
      <div
        aria-hidden="true"
        id={`${props.id}-info`}
        className={`${
          props.maxCharacter - props.value.length < 0
            ? "govuk-error-message"
            : ""
        }`}
      >
        {getCharacterRemainingText(props.maxCharacter - props.value.length)}
      </div>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {getCharacterRemainingText(characterCount)}
      </div>
    </div>
  );
};
