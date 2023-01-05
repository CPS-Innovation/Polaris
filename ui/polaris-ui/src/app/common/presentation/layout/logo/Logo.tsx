import { FC } from "react";
import pngLogo from "./logo.png";

type LogoProps = {
  height: number;
};

export const Logo: FC<LogoProps> = ({ height }) => (
  <img src={pngLogo} alt="CPS logo" height={height} />
);
