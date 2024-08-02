import { ReClassifyProvider } from "./context/ReClassifyProvider";
import { ReclassifyStages } from "./ReclassifyStages";

type ReclassifyProps = {
  presentationTitle?: string;
  handleCancelReclassify: () => void;
};

export const Reclassify: React.FC<ReclassifyProps> = ({
  handleCancelReclassify,
  presentationTitle = "Asset Rec 1",
}) => {
  return (
    <div>
      <ReClassifyProvider>
        <ReclassifyStages
          handleCancelReclassify={handleCancelReclassify}
          presentationTitle={presentationTitle}
        />
      </ReClassifyProvider>
    </div>
  );
};
