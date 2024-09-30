const template = document.createElement("template");
template.innerHTML = `
  <style>
  @import "https://as-web-rumpole-ux-dev.azurewebsites.net/public/stylesheets/application.css";

 main.case-files {padding-top:0 !important}

 #secondary-navigation {
    margin-bottom: 1em;
 }
 #secondary-navigation ul li a {
    color: #1d70b8;
    text-decoration: none;
    font-size: 1.3em;
}
#secondary-navigation ul li {
    box-sizing: border-box;
    padding: 0 15px;
    position: relative;
}

#secondary-navigation ul li.selected:before {
    display: inline-block;
    content: '';
    height: 5px;
    left: 0;
    right: 0;
    background: #1d70b8;
    position: absolute;
    bottom: -0.7em;
}


#secondary-navigation ul:first-child {
     padding-left:0;
}

#secondary-navigation ul li.current {
    font-weight:bold;
}

#global-navigation ul {
    position: static;
    left: inherit;
    right: 0;

}

#global-navigation ul li{
    z-index: 9999;
}


.classicNav:after {
    content: "";
    width: 0px;
    height: 0px;
    border-left: 8px solid transparent;
    border-right: 8px solid transparent;
    border-top: 8px solid #1d70b8;
    position: absolute;
    //left: 135px;
    top: 21px;
}

#test-nav {
    float: left;
    display: inline-block;
}

#test-nav h1 {
    position: relative;
}

#test-nav h1:before {
    content: '';
    position: absolute;
    top: 0;
    bottom: 0;
    background: #f3f2f1;
    left: -10px;
    z-index: 1;
    right: 0;
}

#test-nav h1:after {
    position: absolute;
    content: '';
    right: -20px;
    top: 0;
    bottom: 0;
    border-left: 20px solid #f3f2f1;
    border-bottom: 22px solid transparent;
    border-top: 22px solid transparent;    
}

/* Hide the dropdown menu by default */
.dropdown-menu {
    display: none;
    position: absolute !important;
    z-index:99;
    top: 100%;
    background:#F5F5F5;
    right: 30px;
    list-style: none;
    padding: 0;
    margin: 0;
    min-width:250px;
    box-shadow: 10px 10px 10px 10px rgba(0, 0, 0, 0.2);
    border-top: 5px solid #1d70b8;
}

.dropdown-menu li  {
    display:block;
    padding: 10px 15px;
    font-weight:400;
}

.dropdown-menu li a {
    font-weight:400 !important;
}

.menu-item.show .classicNav:after {
    transform: rotate(180deg);
}

/* Show the dropdown menu when the parent has the .show class */
.menu-item.show .dropdown-menu {
    display: flex;
    flex-direction: column;
}

:host {
  display: none;
  font-size:80%;
}

:host([open]) {
  display: block;
}

:host .govuk-width-container {
  margin-left: 10px !important;
}

#global-navigation {
    box-shadow: 0 0 0 100vmax #f3f2f1;
    clip-path: inset(0 -100vmax); 
}
#secondary-navigation {
     box-shadow: 0 0 0 100vmax #f3f2f1;
    clip-path: inset(0 0 0 -100vmax); 
}

hr {
  padding:0;
  margin:0 -20em;
}

nav {
  border-bottom: 0 !important;
}

  </style>
  <div class="nav-a-tron-container">

    <nav class="cps-bar-wrapper" id="global-navigation" style="margin-bottom:0">
        <div class="govuk-width-container">
            <div class="govuk-grid-row">
                <div class="govuk-grid-column-full">
                    <ul style="display:flex; width:99%" class="">
                        <li class="dashboard"><a href="A-dashboard">Home</a></li>
                        <li class="my-tasks selected"><a href="https://www.figma.com/proto/zNeiezQ0KQCAlE5HQM7RuG/Work-Management-App-%E2%80%93-Beta-5th-June?node-id=211-61999&amp;viewport=753%2C1538%2C0.14&amp;t=y00WRFqyQXAsYrFH-0&amp;scaling=min-zoom&amp;content-scaling=fixed&amp;starting-point-node-id=212%3A70779&amp;show-proto-sidebar=1">Tasks</a></li>
                        <li class="my-cases"><a href="#">Cases</a></li>
                    </ul>
                </div>
            </div>
        </div>
    </nav>
    <hr />
    <nav class="cps-bar-wrapper" id="secondary-navigation" style="background:#fff; z-index:9; margin-bottom: 0">
        <div class="govuk-width-container">
            <div class="govuk-grid-row">
                <div class="govuk-grid-column-full">
                    <div class="case-info" id="test-nav">
                        <h1 class="govuk-heading-s" style="margin:0; padding: 0.55em 0.5em 0.55em 0; background: #f3f2f1;">
                            <span style="position: relative; z-index:2;">McLove, Eoin</span>
                        </h1>
                    </div>
                    <ul style="display:flex; margin:10px 0; width:30%; float:left; padding:0 0 0 1em; list-style: none;">
                        <li class="sub-overview"><a href="http://localhost:3000/polaris-ui/nav-a-tron-other-app.html?nav">Overview</a></li>
                        <li class="sub-case selected dropdown">
                            <a href="http://localhost:3000/polaris-ui/case-details/45CV2911222/2149310?nav" class="third-level-trigger">Materials</a>
                            <ul class="dropdown-menu third-level" style="display: none;">
                                <li><a href="./C-casefile-1" class="disabled" disabled="">Case materials</a></li>
                                <li><a href="https://www.figma.com/proto/Oemk95xWfmYbNzgi6grtdO/CM?page-id=595%3A6946&amp;node-id=729-4169&amp;viewport=-1533%2C504%2C0.3&amp;t=y6f4KoKfmwqMAjwQ-1&amp;scaling=min-zoom&amp;content-scaling=fixed&amp;starting-point-node-id=729%3A4169';">Bulk UM Classification</a></li>
                            </ul>
                        </li>
                        <li class="sub-review"><a href="#">Review</a></li>
                        <li><a href="#" onclick="launchTriage()">Triage</a></li>
                    </ul>
                  </div>
              </div>
          </div>
      </nav>
      <hr />
  </div>
`;

class NavATron extends HTMLElement {
  connectedCallback() {
    this.attachShadow({ mode: "open" });
    this.shadowRoot.appendChild(template.content.cloneNode(true));

    fetch("http://localhost:7075/api/urns/45CV2911222/cases/2149310", {
      credentials: "include",
    })
      .then((response) => response.json())
      .then((data) => console.log(data));
  }
}

const url = window.location.href;
const inNavMode = url.includes("?nav") || url.includes("&nav");

let navATron;
if (inNavMode) {
  customElements.define("nav-a-tron", NavATron);

  navATron = document.createElement("nav-a-tron");

  const script = document.scripts[document.scripts.length - 1];
  script.parentElement.insertBefore(
    document.createElement("nav-a-tron"),
    script
  );
}

const observer = new MutationObserver((mutationList, o) => {
  for (const mutation of mutationList) {
    if (mutation.type === "childList") {
      const navATronRoot = document.getElementById("nav-a-tron");
      if (inNavMode) {
        if (
          navATronRoot &&
          !navATronRoot.getElementsByTagName("nav-a-tron").length // already built
        ) {
          setTimeout(() => {
            navATron.setAttribute("open", "true");
          }, 200);
          navATronRoot.append(navATron);
        }
      } else {
        navATronRoot && navATronRoot.remove();
      }
    }
  }
});

observer.observe(document.body, {
  childList: true,
  subtree: true,
});
