import { r as registerInstance, a as createEvent, h } from './index-7a261123.js';

const NavLink = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.cpsGlobalNavEvent = createEvent(this, "cps-global-nav-event", 7);
        this.emitEvent = () => {
            this.cpsGlobalNavEvent.emit(this.href);
        };
        this.launchNewTab = () => {
            window.open(this.href, "_blank", "noopener,noreferrer");
        };
    }
    render() {
        const mode = this.disabled || !this.href ? "disabled" : this.openInNewTab ? "new-tab" : !this.href.startsWith("http") ? "emit-event" : "standard";
        const renderLink = () => {
            switch (mode) {
                case "disabled":
                    return (h("a", { class: "govuk-link disabled", "aria-disabled": true, href: this.href }, this.label));
                case "new-tab":
                    return (h("button", { class: "linkButton", onClick: this.launchNewTab }, this.label));
                case "emit-event":
                    return (h("button", { class: "linkButton", onClick: this.emitEvent }, this.label));
                default:
                    return (h("a", { class: "govuk-link", href: this.href }, this.label));
            }
        };
        return h("li", { class: this.selected ? "selected" : "" }, renderLink());
    }
};

export { NavLink as nav_link };

//# sourceMappingURL=nav-link.entry.js.map