function getPolarisUrl() {
  var polarisUrl = "/polaris";
  if (window.iCaseId && window.sURN) {
    polarisUrl =
      polarisUrl +
      "?polaris-ui-url=/materials-ui/" +
      window.sURN +
      "/" +
      window.iCaseId + "/materials";
  }
  return polarisUrl;
}
function addPolarisButton() {
  var tableRowElement = document.getElementById("trTitleBar");
  var secondChild = tableRowElement.children[1];
  var td = document.createElement("td");
  var pLink = document.createElement("a");
  pLink.style.whiteSpace = "nowrap";
  pLink.href = getPolarisUrl();
  pLink.target = "_blank";
  pLink.innerText = "Open in Manage Materials";
  pLink.style.color = "white";
  td.style.width = "100px";
  td.appendChild(pLink);
  tableRowElement.insertBefore(td, secondChild);
}
addPolarisButton();