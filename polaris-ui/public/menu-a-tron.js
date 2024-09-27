// Select the node that will be observed for mutations
const targetNode = document.getElementById("root");

// Callback function to execute when mutations are observed
const callback = (mutationList, observer) => {
  for (const mutation of mutationList) {
    if (mutation.type === "childList") {
      ensureMenu(document.getElementById("menu-a-tron"));
    }
  }
};

// Create an observer instance linked to the callback function
const observer = new MutationObserver(callback);

// Start observing the target node for configured mutations
observer.observe(targetNode, { childList: true, subtree: true });

function ensureMenu(el) {
  const built = el.attributes["menu-a-tron-built"];
  if (built) {
    return;
  }

  const createMenuItem = (txt) => {
    const menuItem = document.createElement("div");
    menuItem.append(document.createTextNode(txt));
    return menuItem;
  };
  el.appendChild(createMenuItem(" . "));
  el.appendChild(createMenuItem("Menu item a"));
  el.appendChild(createMenuItem("Menu item b"));
  el.appendChild(createMenuItem("Menu item c"));

  console.log("hi");
  el.setAttribute("menu-a-tron-built", "true");
}
