import { r as registerInstance, h } from './index-818db758.js';

const NavLink = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("li", { key: '9042e7e019268947a8bcb9fa5d458330fd8a7842', class: this.selected ? "selected" : "" }, h("a", { key: 'e3bbdd477367448ac41b1ec1833f8cdfa2762484', class: `govuk-link ${this.selected ? "disabled" : ""}`, "aria-disabled": this.selected, href: this.href }, this.label)));
    }
};

export { NavLink as nav_link };

//# sourceMappingURL=nav-link.entry.js.map