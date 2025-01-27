import { r as registerInstance, h } from './index-818db758.js';

const DropDown = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.menuAlignment = "left";
        this.handleLabelClick = () => {
            this.opened = !this.opened;
        };
    }
    checkForClickOutside(ev) {
        if (!ev.composedPath().includes(this.topLevelHyperlink)) {
            this.opened = false;
        }
    }
    render() {
        return (h("li", { key: '619c2909743b17efc381b3848104bd5e0c1d8234', class: `dropdown ${this.opened ? "active" : ""} ${this.links.some(link => link.selected) ? "selected" : ""}` }, h("button", { key: '12044cea249d4b421fdc0714b596610b01d0b1ee', class: "linkButton", onClick: this.handleLabelClick, ref: el => (this.topLevelHyperlink = el) }, this.label), h("ul", { key: 'c04739efb12e8154c2b3158c59e2f6ceb21d41c6', class: this.menuAlignment == "right" ? "align-right" : "" }, this.links.map(link => (h("nav-link", Object.assign({}, link)))))));
    }
};

export { DropDown as drop_down };

//# sourceMappingURL=drop-down.entry.js.map