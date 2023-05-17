function getPolarisUrl() {
  var domainUrl = "https://as-web-polaris-dev.azurewebsites.net/case-details/";
  var polarisUrl = "";
  if (window.iCaseId && window.sURN) {
    polarisUrl = domainUrl + window.sURN + "/" + window.iCaseId;
  } else {
    var tableRowElement = document.getElementById("trTitleBar");
    var caseUrnTdElement = tableRowElement.children[1];
    var caseUrn = caseUrnTdElement.textContent;
    var caseIdElement = document.getElementById("p-caseid");
    var caseId = caseIdElement.textContent;
    polarisUrl = domainUrl + caseUrn + "/" + caseId;
  }

  return polarisUrl;
}

function addPolarisButton() {
  var tableRowElement = document.getElementById("trTitleBar");
  var td = document.createElement("td");
  var pLink = document.createElement("a");
  pLink.href = getPolarisUrl();
  pLink.target = "_blank";
  pLink.textContent = "Open in Polaris";
  pLink.style.color = "white";
  td.style.width = "60px";
  td.appendChild(pLink);

  const secondChild = tableRowElement.children[1];

  tableRowElement.insertBefore(td, secondChild);
}

addPolarisButton();
