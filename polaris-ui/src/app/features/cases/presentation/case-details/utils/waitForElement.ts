export const waitForElement = async (
  getElement: () => Element | undefined,
  timeout: number = 5000
): Promise<Element> => {
  return new Promise((resolve, reject) => {
    const startTime = Date.now();
    const checkForElement = () => {
      const targetElement = getElement();
      if (targetElement) {
        resolve(targetElement);
      } else {
        if (Date.now() - startTime >= timeout) {
          reject(new Error("timeout reached"));
        } else {
          requestAnimationFrame(checkForElement);
        }
      }
    };

    requestAnimationFrame(checkForElement);
  });
};
