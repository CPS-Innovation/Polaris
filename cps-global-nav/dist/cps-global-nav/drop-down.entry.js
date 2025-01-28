import { r as registerInstance, h } from './index-7a261123.js';

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
        return (h("li", { key: '62413672c3116db30fc051f61b9c022bf80c45e6', class: `dropdown ${this.opened ? "active" : ""} ${this.links.some(link => link.selected) ? "selected" : ""}` }, h("button", { key: 'ae528c4ac9c0a3fa87c2ef63bb7a48e742249fcb', class: "linkButton", onClick: this.handleLabelClick, ref: el => (this.topLevelHyperlink = el) }, this.label), h("ul", { key: '25fd3a08deee7ecada03353cf230ca01abb1dbff', class: this.menuAlignment == "right" ? "align-right" : "" }, this.links.map(link => (h("nav-link", Object.assign({}, link)))))));
    }
};

export { DropDown as drop_down };

//# sourceMappingURL=drop-down.entry.js.map