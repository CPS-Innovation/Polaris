import React, { useState } from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { useLastFocus } from "./useLastFocus";

describe("useLastFocus hook", () => {
  const testSetUp = () => {
    jest.useFakeTimers();
    const TestComponent = () => {
      const [showModal, setShowModal] = useState(false);
      const [showModalOpenButton, setShowModalOpenButton] = useState(true);
      return (
        <div>
          {showModalOpenButton && (
            <button onClick={() => setShowModal(true)}>Open Modal</button>
          )}
          {showModal && (
            <ModalComponent
              handleClose={setShowModal}
              setShowModalOpenButton={setShowModalOpenButton}
            />
          )}
          <button id="default-focus-button">Default Focus</button>
        </div>
      );
    };

    type ModalComponentProps = {
      handleClose: (value: boolean) => void;
      setShowModalOpenButton: (value: boolean) => void;
    };
    const ModalComponent: React.FC<ModalComponentProps> = ({
      handleClose,
      setShowModalOpenButton,
    }) => {
      useLastFocus("#default-focus-button");

      return (
        <div>
          <button onClick={() => handleClose(false)}>Close Modal</button>
          <button onClick={() => setShowModalOpenButton(false)}>
            Hide Open Modal Button
          </button>
        </div>
      );
    };

    render(<TestComponent />);
  };

  it("On unmounting the Modal component, should put the focus back to the last active element if it is present in the parent component", () => {
    testSetUp();

    const openModalButtonElement = screen.getByText("Open Modal");
    openModalButtonElement.focus();
    fireEvent.click(openModalButtonElement);
    jest.advanceTimersByTime(10);
    expect(openModalButtonElement).toHaveFocus();

    const closeModalButtonElement = screen.getByText("Close Modal");
    closeModalButtonElement.focus();
    expect(closeModalButtonElement).toHaveFocus();

    fireEvent.click(closeModalButtonElement);
    jest.advanceTimersByTime(10);
    expect(openModalButtonElement).toHaveFocus();
  });

  it("On unmounting the Modal component, should put the focus back to defaultFocus element if the last active element in not present in the  parent component", () => {
    testSetUp();

    const openModalButtonElement = screen.getByText("Open Modal");
    openModalButtonElement.focus();
    fireEvent.click(openModalButtonElement);
    expect(openModalButtonElement).toHaveFocus();

    const hideModalButtonElement = screen.getByText("Hide Open Modal Button");
    hideModalButtonElement.focus();
    expect(hideModalButtonElement).toHaveFocus();

    fireEvent.click(hideModalButtonElement);
    const closeModalButtonElement = screen.getByText("Close Modal");
    closeModalButtonElement.focus();
    expect(closeModalButtonElement).toHaveFocus();
    fireEvent.click(closeModalButtonElement);

    const defaultFocus = screen.getByText("Default Focus");
    jest.advanceTimersByTime(10);
    expect(defaultFocus).toHaveFocus();
  });
});
