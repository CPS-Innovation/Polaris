import { ReClassifyProvider } from "./context/ReClassifyProvider";
import { ReclassifyPage1 } from "./ReclassifyPage1";
import { ReclassifyPage2 } from "./ReclassifyPage2";

type ReclassifyProps = {};

export const Reclassify = () => {
  return (
    <div>
      <ReClassifyProvider>
        <ReclassifyPage1 />
        <ReclassifyPage2 />
      </ReClassifyProvider>
    </div>
  );
};
