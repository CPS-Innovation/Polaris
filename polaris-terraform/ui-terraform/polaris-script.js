function getPolarisUrl() {
  var domainUrl =
    window.location.protocol +
    "//" +
    window.location.host +
    "/polaris-ui/case-details/";
  var polarisUrl = "";
  if (window.iCaseId && window.sURN) {
    polarisUrl = domainUrl + window.sURN + "/" + window.iCaseId;
  }
  return polarisUrl;
}
function addPolarisButton() {
  var tableRowElement = document.getElementById("trTitleBar");
  var secondChild = tableRowElement.children[1];
  var td = document.createElement("td");
  var pLink = document.createElement("a");
  pLink.href = getPolarisUrl();
  pLink.target = "_blank";
  pLink.innerText = "Open in Polaris";
  pLink.style.color = "white";
  td.style.width = "100px";
  td.appendChild(pLink);
  tableRowElement.insertBefore(td, secondChild);
}
addPolarisButton();
