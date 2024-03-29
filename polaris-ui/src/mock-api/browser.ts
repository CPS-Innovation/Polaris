// src/mocks/browser.js
import { setupWorker, rest } from "msw";
import { setupHandlers } from "./handlers";
import { MockApiConfig } from "./MockApiConfig";

export type SetupMockApi = typeof setupMockApi;

export const setupMockApi = async (config: MockApiConfig) => {
  if ((window as any).msw) {
    return;
  }

  const worker = setupWorker(...setupHandlers(config));
  await worker.start({
    serviceWorker: {
      // Points to the custom location of the Service Worker file.
      url: `${config.publicUrl}/mockServiceWorker.js`,
    },
  });

  (window as any).msw = { worker, rest }; // attach to window for cypress testing purposes
};
