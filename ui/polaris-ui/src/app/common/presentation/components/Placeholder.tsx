type Props = {
  height: number;
  width?: number;
  marginTop?: number;
  marginBottom?: number;
  marginLeft?: number;
  marginRight?: number;
  backgroundColor?: string;
};

export const Placeholder: React.FC<Props> = ({
  height,
  width,
  marginTop = 5,
  marginBottom = 5,
  marginLeft = 0,
  marginRight = 0,
  backgroundColor = "#ffffff",
}) => {
  return (
    <div
      style={{
        height: height - 2,
        marginTop,
        marginBottom,
        marginLeft,
        marginRight,
        backgroundColor,
        border: "1px dashed #888888",
        width,
      }}
    ></div>
  );
};
