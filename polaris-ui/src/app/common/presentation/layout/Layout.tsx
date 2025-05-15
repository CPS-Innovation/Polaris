import React, { useEffect, useRef } from "react";
import { useLocation } from "react-router-dom";
import { useUserDetails } from "../../../auth";
import { useUserGroupsFeatureFlag } from "../../../auth/msal/useUserGroupsFeatureFlag";
import { SkipLink } from "../components";
import classes from "./Layout.module.scss";

type LayoutProps = {
  isWide?: boolean;
  children: React.ReactNode;
};

export const Layout: React.FC<LayoutProps> = ({ isWide, children }) => {
  const containerCssClass = isWide
    ? classes["cps-width-container-wide"]
    : "govuk-width-container";
  const location = useLocation();
  const userDetails = useUserDetails();
  const skipLinkSiblingRef = useRef(null);

  useEffect(() => {
    /*This is a way to bring the focus back to the top of the page
    whenever location change. We bring the focus to the dummy element just above the skip link and the call the blur(for the screen reader not to say it loud),
    so that the next tabbable element will be skip link when user start tabbing on the page
    */
    if (skipLinkSiblingRef.current) {
      (skipLinkSiblingRef.current as HTMLButtonElement).focus();
      (skipLinkSiblingRef.current as HTMLButtonElement).blur();
    }
  }, [location.pathname]);
  const { globalNav } = useUserGroupsFeatureFlag();

  return (
    <>
      <div ref={skipLinkSiblingRef} tabIndex={-1} />
      {globalNav ? (
        <div className={containerCssClass}>
          <cps-global-header></cps-global-header>
        </div>
      ) : (
        <>
          <SkipLink href="#main-content">Skip to main content</SkipLink>
          <header
            className="govuk-header "
            role="banner"
            data-module="govuk-header"
          >
            <div className={`govuk-header__container`}>
              <div className={containerCssClass}>
                <div className={`govuk-header__logo ${classes.logo} `}>
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
                  <span className="govuk-header__link--homepage">
                    Casework App
                  </span>
                </div>
              </div>
            </div>
          </header>
        </>
      )}
      <div className={`${containerCssClass} ${classes["cps-main-container"]}`}>
        {children}
      </div>
      <footer className="govuk-footer" role="contentinfo">
        <div
          className={`${containerCssClass}  ${classes["cps-footer-container"]}`}
        >
          <h2 className="govuk-visually-hidden">Support links</h2>
          <ul className="govuk-footer__inline-list">
            <li className="govuk-footer__inline-list-item">
              <a className="govuk-footer__link" href="#/">
                Accessibility
              </a>
            </li>
          </ul>
          <div style={{ marginLeft: 15 }}>{userDetails.username}</div>
        </div>
      </footer>
    </>
  );
};
