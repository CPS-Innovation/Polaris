import { waitForElement } from "./waitForElement";

describe("waitForElement", () => {
  it("should resolve with the correct element when found", async () => {
    document.body.innerHTML = `
      <div class="pdfViewer">abc1</div>
      <div class="pdfViewer">abc2</div>
    `;

    const element = await waitForElement(
      () => document.querySelectorAll(".pdfViewer")[1]
    );
    expect(element.textContent).toBe(`abc2`);
  });

  it("should handle invalid tabIndex gracefully", async () => {
    document.body.innerHTML = `
      <div class="pdfViewer"></div>
    `;
    await expect(
      waitForElement(() => document.querySelectorAll(".pdfViewer")[5])
    ).rejects.toThrow("max retries reached");
  });

  it("should not resolve if the element is not found immediately but resolves later", async () => {
    const requestAnimationFrameSpy = jest.spyOn(
      global,
      "requestAnimationFrame"
    );
    document.body.innerHTML = "";
    setTimeout(() => {
      document.body.innerHTML = `
            <div class="pdfViewer"></div>
            <div class="pdfViewer"></div>
          `;
    }, 1);

    const element = await waitForElement(
      () => document.querySelectorAll(".pdfViewer")[1]
    );
    expect(requestAnimationFrameSpy).toBeCalledTimes(1);
    expect(element).toBe(document.querySelectorAll(".pdfViewer")[1]);
    jest.clearAllMocks();
  });
});
