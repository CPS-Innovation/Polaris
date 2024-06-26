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
  disabled?: boolean;
  maxCharacters: number;
  errorMessage?: {
    children: React.ReactNode;
  };
  onChange: (value: React.ChangeEvent<HTMLTextAreaElement>) => void;
};
export const CharacterCount: React.FC<CharacterCountProps> = (props) => {
  const { maxCharacters, value } = props;
  const [characterCount, setCharacterCount] = useState(maxCharacters);
  useEffect(() => {
    const timer = setTimeout(() => {
      setCharacterCount(maxCharacters - value.length);
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
          maxCharacters - value.length < 0 ? "govuk-error-message" : ""
        }`}
      >
        {getCharacterRemainingText(maxCharacters - value.length)}
      </div>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {getCharacterRemainingText(characterCount)}
      </div>
    </div>
  );
};
