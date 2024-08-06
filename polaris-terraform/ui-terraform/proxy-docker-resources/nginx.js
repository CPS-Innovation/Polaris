async function polarisAuthRedirect(r) {
  const redirectHostAddress = r.variables.redirectHostAddress ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  const referer = encodeURIComponent(r.headersIn.Referer ?? "")
  const polarisUiUrl = encodeURIComponent(r.args["polaris-ui-url"] ?? "")
  const q = r.args["q"] ?? ""
  r.return(
      302,
      `${redirectHostAddress}/init?cookie=${cookie}&referer=${referer}&polaris-ui-url=${polarisUiUrl}&q=${q}`
  )
}

function taskListAuthRedirect(r) {
  const taskListHostAddress = r.variables["taskListHostAddress"] ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  r.return(
      302,
      `${taskListHostAddress}/WorkManagementApp/Redirect?Cookie=${cookie}`
  )
}

function fetchClassicIpAddress(r) {
  let lookupKey = retrieveDestinationKey(r, "CLASSIC");
  return retrieveDestinationValue(r, 0, lookupKey);
}

function fetchClassicHostName(r) {
  let lookupKey = retrieveDestinationKey(r, "CLASSIC");
  return retrieveDestinationValue(r, 1, lookupKey);
}

function fetchModernIpAddress(r) {
  let lookupKey = retrieveDestinationKey(r, "MODERN");
  return retrieveDestinationValue(r, 0, lookupKey);
}

function fetchModernHostName(r) {
  let lookupKey = retrieveDestinationKey(r, "MODERN");
  return retrieveDestinationValue(r, 1, lookupKey);
}

function retrieveDestinationKey(r, classicOrModernFlag) {
  const CORSHAM_FRAGMENT = "CPSAC";
  const FARNBOROUGH_FRAGMENT = "CPSAF";
  const cookies = decodeURIComponent(r.headersIn.Cookie).split(/;\s*/);
  const isCorshamCookiePresent = retrieveLoadBalancerTarget(cookies, CORSHAM_FRAGMENT);
  const isFarnboroughCookiePresent = retrieveLoadBalancerTarget(cookies, FARNBOROUGH_FRAGMENT);
  const clientIpAddress = r.variables["remoteUserIp"];
  const selectedEnvironment = r.headersIn["cms-selected-environment"];
  const hostNameFlag = retrieveHostNameFlag(r, cookies, selectedEnvironment); // returns e.g. CIN3
  const corshamOrFarnboroughFlag = determineLoadBalancerTargetChoice(r.headersIn["Cms-Auth-Values"], isCorshamCookiePresent, isFarnboroughCookiePresent, clientIpAddress);

  r.variables["loadBalancerTarget"] = corshamOrFarnboroughFlag;
  r.log(`Remote IP assessed ${clientIpAddress}`);
  r.log(`Constructed lookup key: ${classicOrModernFlag}_${corshamOrFarnboroughFlag}_${hostNameFlag}`)

  // construct a env variable key 
  return `${classicOrModernFlag}_${corshamOrFarnboroughFlag}_${hostNameFlag}`; // e.g. "CLASSIC_CORS_CIN3"
}

function retrieveDestinationValue(r, idx, key) {
  let foundEnvSettings, destinationValue;
  let loadBalancerTarget = r.variables["loadBalancerTarget"];

  foundEnvSettings = process.env[key];

  // if the key is present it would be something like "10.2.177.3;cin2.cps.gov.uk"
  if (typeof(foundEnvSettings) == "string" && foundEnvSettings !== ""){
    // we have a value for this key
    destinationValue = foundEnvSettings.split(";")[idx];
  } else {
    //no value has been returned, we need to fall back to a default
    const classicOrModernFlag = key.split("_")[0];
    const defaultEnvSettings = process.env[`${classicOrModernFlag}_${loadBalancerTarget}_DEFAULT`];
    destinationValue = defaultEnvSettings.split(";")[idx];
  }

  return destinationValue;
}

function retrieveLoadBalancerTarget(cookies, target) {
  const cookieSearch = cookies.filter(c => c.toUpperCase().indexOf(target) > -1);

  let result = false;
  if (cookieSearch[0] !== undefined) {
    result = true;
  }
  return result;
}

function retrieveHostNameFlag(r, cookies, selectedEnvironment) {
  if (selectedEnvironment != null && selectedEnvironment !== "") {
    r.log(`Selected Environment: [${selectedEnvironment}]`)    
    return selectedEnvironment.toUpperCase();
  }
  else {
    const cookieSearch = cookies.filter(c => c.toUpperCase().indexOf("BIGIPSERVER") > -1);

    let result = "DEFAULT";
    if (cookieSearch[0] !== undefined) {
      const str = cookieSearch[0];
      const startPos = str.toUpperCase().indexOf(".CPS.GOV.UK");
      result = retrieveHostName(str, startPos - 1);
    }
    return result;
  }
}

function retrieveHostName(str, startPoint) {
  let hostName = "";
  for (let i = startPoint; i >= 0; i--) {
    if (str[i] === "-") {
      break;
    }
    hostName += str[i];
  }
  return hostName.toUpperCase().split("").reverse().join("");
}

function determineLoadBalancerTargetChoice(cmsAuthValues, isCorshamCookiePresent, isFarnboroughCookiePresent, clientIpAddress) {
  const CORSHAM_FLAG = "CORS";
  const FARNBOROUGH_FLAG = "FARN";

  if (cmsAuthValues != null && cmsAuthValues !== '') {
    const cmsAuthObj = JSON.parse(cmsAuthValues);
    if (typeof(cmsAuthObj) != 'undefined' && cmsAuthObj != null)
    {
      const preferredLoadBalancerTarget = cmsAuthObj.PreferredLoadBalancerTarget;
      if (preferredLoadBalancerTarget !== "") {
        return preferredLoadBalancerTarget === "0" ? CORSHAM_FLAG : FARNBOROUGH_FLAG;
      }

      const ipAddress = clientIpAddress === "" ? cmsAuthObj.UserIpAddress : clientIpAddress;
      const isCorshamLeaningIpAddress = isCorshamLeaningIpAddress(ipAddress);

      if (isFarnboroughCookiePresent && isCorshamCookiePresent) {
        return isCorshamLeaningIpAddress ? CORSHAM_FLAG : FARNBOROUGH_FLAG;
      }

      if (isFarnboroughCookiePresent) {
        return FARNBOROUGH_FLAG;
      }

      if (isCorshamCookiePresent) {
        return CORSHAM_FLAG;
      }
    }
  }

  return CORSHAM_FLAG;
}

function isCorshamLeaningIpAddress(userIpAddress) {
  return (typeof(userIpAddress) != 'undefined' && userIpAddress !== "" && userIpAddress.startsWith("10.12."));
}

function fetchDefaultClassicCorshamIpAddress() {
  return retrieveSetting("CLASSIC_CORS_DEFAULT", 0);
}

function fetchDefaultClassicCorshamHostName() {
  return retrieveSetting("CLASSIC_CORS_DEFAULT", 1);
}

function fetchDefaultModernCorshamIpAddress() {
  return retrieveSetting("MODERN_CORS_DEFAULT", 0);
}

function fetchDefaultModernCorshamHostName() {
  return retrieveSetting("MODERN_CORS_DEFAULT", 1);
}

function retrieveSetting(key, idx) {
  let settings = process.env[key];
  return settings.split(";")[idx];
}

export default { polarisAuthRedirect, taskListAuthRedirect, fetchClassicIpAddress, fetchClassicHostName, fetchModernIpAddress, fetchModernHostName, fetchDefaultClassicCorshamIpAddress, fetchDefaultClassicCorshamHostName, fetchDefaultModernCorshamIpAddress, fetchDefaultModernCorshamHostName }