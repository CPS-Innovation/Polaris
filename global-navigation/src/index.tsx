import ReactDOM from "react-dom/client";
import "./index.module.scss";
import "../node_modules/govuk-frontend/dist/govuk/govuk-frontend.min.css";
import { GlobalNavigation } from "./GlobalNavigation";

export class WebComponent extends HTMLElement {
  connectedCallback() {
    const mountPoint = document.createElement("div");
    this.attachShadow({ mode: "open" }).appendChild(mountPoint);

    //const name = this.getAttribute("name") || "CPS";
    ReactDOM.createRoot(mountPoint).render(<GlobalNavigation />);
  }
}
customElements.define("global-navigation", WebComponent);
