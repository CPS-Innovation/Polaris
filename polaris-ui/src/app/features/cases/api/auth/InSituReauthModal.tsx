import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import { Button, Modal } from "../../../../common/presentation/components";
import { AuthErrorPage } from "../../../../common/presentation/components/error-pages/AuthErrorPage";

export const InSituReauthModal: React.FC<{
  error: CmsAuthError;
  handleClose: () => void;
}> = ({ error, handleClose }) => {
  return (
    <Modal
      isVisible
      ariaLabel="Warning regarding missing CMS login"
      ariaDescription="A modal window describing the need to have a current CMS login session operating within the same browser that this application is running in"
      handleClose={handleClose}
    >
      <div style={{ padding: "3em 2em 0 3em" }}>
        <AuthErrorPage error={error}>
          <>
            click the button below to continue:
            <br />
            <br />
            <Button isStartButton onClick={handleClose}>
              Continue with the previous action
            </Button>
          </>
        </AuthErrorPage>
      </div>
    </Modal>
  );
};
