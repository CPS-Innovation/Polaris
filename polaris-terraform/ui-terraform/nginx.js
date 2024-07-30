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

function fetchDestinationKey(r) {
  const CORSHAM_FRAGMENT = "CPSAC";
  const FARNBOROUGH_FRAGMENT = "CPSAF";
  const classicOrModernFlag = r.uri.indexOf("/graphql/") > -1 ? "MODERN" : "CLASSIC";
  const cookies = decodeURIComponent(r.headersIn.Cookie).split(/;\s*/);
  const isCorshamCookiePresent = retrieveLoadBalancerTarget(cookies, CORSHAM_FRAGMENT);
  const isFarnboroughCookiePresent = retrieveLoadBalancerTarget(cookies, FARNBOROUGH_FRAGMENT);
  const clientIpAddress = r.headersIn["x-forwarded-for"];
  const hostNameFlag = retrieveHostNameFlag(cookies); // returns e.g. CIN3
  const corshamOrFarnboroughFlag = determineLoadBalancerTargetChoice(r.headersIn["Cms-Auth-Values"], isCorshamCookiePresent, isFarnboroughCookiePresent, clientIpAddress);

  // construct a env variable key 
  return `${classicOrModernFlag}_${corshamOrFarnboroughFlag}_${hostNameFlag}`; // e.g. "CLASSIC_CORS_CIN3"
}

function fetchDestinationIpAddress(r) {
  let lookupKey, foundEnvSettings, destinationIpAddress;

  lookupKey = fetchDestinationKey(r);

  // now look for the env setting for the key
  foundEnvSettings = process.env[lookupKey];

  // if the key is present it would be something like "10.2.177.3;cin2.cps.gov.uk"
  if (typeof(foundEnvSettings) == "object" && foundEnvSettings !== ""){
    // we have a value for this key
    destinationIpAddress = foundEnvSettings.split(";")[0];
  } else {
    //no value has been returned, we need to fall back to a default
    let classicOrModernFlag;
    classicOrModernFlag = foundEnvSettings.split("_")[0];
    
    const defaultEnvSettings = process.env[`${classicOrModernFlag}_DEFAULT`];
    destinationIpAddress = defaultEnvSettings.split(";")[0];
  }

  return destinationIpAddress;
}

function fetchDestinationHostName(r) {
  let lookupKey, foundEnvSettings, destinationHostName;

  lookupKey = fetchDestinationKey(r);

  // now look for the env setting for the key
  foundEnvSettings = process.env[lookupKey];

  // if the key is present it would be something like "10.2.177.3;cin2.cps.gov.uk"
  if (typeof(foundEnvSettings) == "object" && foundEnvSettings !== ""){
    // we have a value for this key
    destinationHostName = foundEnvSettings.split(";")[1];
  } else {
    //no value has been returned, we need to fall back to a default
    let classicOrModernFlag;
    classicOrModernFlag = foundEnvSettings.split("_")[0];
    
    const defaultEnvSettings = process.env[`${classicOrModernFlag}_DEFAULT`];
    destinationHostName = defaultEnvSettings.split(";")[1];
  }

  return destinationHostName;
}

function retrieveLoadBalancerTarget(cookies, target) {
  const cookieSearch = cookies.filter(c => c.toUpperCase().indexOf(target) > -1);

  let result = false;
  if (cookieSearch[0] !== undefined) {
    result = true;
  }
  return result;
}

function retrieveHostNameFlag(cookies) {
  const cookieSearch = cookies.filter(c => c.toUpperCase().indexOf("BIGIPSERVER") > -1);

  let result = "DEFAULT";
  if (cookieSearch[0] !== undefined) {
    const str = cookieSearch[0];
    const startPos = str.toUpperCase().indexOf(".CPS.GOV.UK");
    result = getHostName(str, startPos-1);
  }
  return result;
}

function getHostName(str, startPoint) {
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

export default { polarisAuthRedirect, taskListAuthRedirect, fetchDestinationIpAddress, fetchDestinationHostName }
