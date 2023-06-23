import React from "react";
import { useUserDetails } from "../../../auth";
import { SkipLink } from "../components";
import classes from "./Layout.module.scss";

type LayoutProps = {
  isWide?: boolean;
};

export const Layout: React.FC<LayoutProps> = ({ isWide, children }) => {
  const containerCssClass = isWide
    ? classes["cps-width-container-wide"]
    : "govuk-width-container";

  const userDetails = useUserDetails();

  return (
    <>
      <SkipLink href="#main-content">Skip to main content</SkipLink>

      <header
        className="govuk-header "
        role="banner"
        data-module="govuk-header"
      >
        <div className={`govuk-header__container ${containerCssClass}`}>
          <div className={`govuk-header__logo ${classes.logo}`}>
            <a
              href="/"
              className="govuk-header__link govuk-header__link--homepage"
              data-testid="link-homepage"
            >
              <span className="govuk-header__logotype">
                <span className="govuk-header__logotype-text">
                  Crown Prosecution Service
                </span>
              </span>
            </a>
            <span className="govuk-header__link--homepage">Polaris</span>
          </div>
        </div>
      </header>

      <div
        id="main-content"
        className={`${containerCssClass} ${classes["cps-main-container"]}`}
      >
        {children}
      </div>

      <footer className="govuk-footer" role="contentinfo">
        <div
          className={`${containerCssClass}  ${classes["cps-footer-container"]}`}
        >
          <h2 className="govuk-visually-hidden">Support links</h2>
          <ul className="govuk-footer__inline-list">
            <li className="govuk-footer__inline-list-item">
              {/** Todo : replace footer links with correct href urls **/}
              <a className="govuk-footer__link" href="#/">
                Privacy
              </a>
            </li>

            <li className="govuk-footer__inline-list-item">
              <a className="govuk-footer__link" href="#/">
                Cookies
              </a>
            </li>

            <li className="govuk-footer__inline-list-item">
              <a className="govuk-footer__link" href="#/">
                Accessibility
              </a>
            </li>

            <li className="govuk-footer__inline-list-item">
              <a
                className="govuk-footer__link"
                href="/prototype-admin/clear-data"
              >
                Clear data
              </a>
            </li>
          </ul>
          <div style={{ marginLeft: 15 }}>{userDetails.username}</div>
        </div>
      </footer>
    </>
  );
};
