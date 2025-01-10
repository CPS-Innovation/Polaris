export const waitForElement = async (
  getElement: () => Element | undefined,
  maxRetry: number = 4
): Promise<Element> => {
  return new Promise((resolve, reject) => {
    let retry = 0;
    const checkForElement = () => {
      const targetElement = getElement();
      if (targetElement) {
        resolve(targetElement);
      } else {
        retry++;

        if (retry >= maxRetry) {
          reject(new Error("max retries reached"));
        } else {
          requestAnimationFrame(checkForElement);
        }
      }
    };

    requestAnimationFrame(checkForElement);
  });
};
