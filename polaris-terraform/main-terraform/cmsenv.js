function getDomainFromCookie(r) {
    let cookie = r.headersIn.Cookie || '';
    let domainMatch = cookie.match(/([a-z0-9]+)\.cps\.gov\.uk/);
    
    return domainMatch[0];
}

function proxyDestinationCorsham(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsIpCorsham'];
}

function proxyDestinationCorshamInternal(r)
{
    return proxyDestinationCorsham(r);
}

function proxyDestinationModernCorsham(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsModernIpCorsham'];
}

function proxyDestinationModernCorshamInternal(r)
{
    return proxyDestinationModernCorsham(r);
}

function proxyDestinationFarnborough(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsIpFarnborough'];
}

function proxyDestinationFarnboroughInternal(r)
{
    return proxyDestinationFarnborough(r);
}

function proxyDestinationModernFarnborough(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsModernIpFarnborough'];
}

function proxyDestinationModernFarnboroughInternal(r)
{
    return proxyDestinationModernFarnborough(r);
}

function upstreamCmsDomainName(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsDomainName'];
}

function upstreamCmsModernDomainName(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsModernDomainName'];
}

function upstreamCmsServicesDomainName(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsServicesDomainName'];
}

function upstreamCmsIpCorsham(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsIpCorsham'];
}

function upstreamCmsModernIpCorsham(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsModernIpCorsham'];
}

function upstreamCmsIpFarnborough(r) 
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsIpFarnborough'];
}

function upstreamCmsModernIpFarnborough(r) {
    let cmsEnv = __getCmsEnv(r);
    return r.variables[cmsEnv + 'UpstreamCmsModernIpFarnborough'];
}

function replaceCmsDomains(r, data, flags)
{
    __replaceCmsDomainsGeneric(r, data, flags, r.variables.host);
}

function replaceCmsDomainsAjaxViewer(r, data, flags)
{
    __replaceCmsDomainsGeneric(r, data, flags, r.variables.websiteHostname);
}

function cmsMenuBarFilters(r, data, flags)
{
    data = __addAppLaunchButtonsToMenuBar(r, data, flags);
    replaceCmsDomains(r, data, flags);
}

function devLoginEnvCookie(r)
{
    let cmsEnv = __getCmsEnvCookieOut(r);
    let cookies = r.headersOut['Set-Cookie'];
    cookies.push('__CMSENV=' + cmsEnv + '; path=/');
    r.headersOut['Set-Cookie'] = cookies;
}

function __addAppLaunchButtonsToMenuBar(r, data, flags) {
    data = data.replace(
        new RegExp('objMainWindow\\.top\\.frameData\\.objMasterWindow\\.top\\.frameServerJS\\.POLARIS_URL', 'g'),
        '"/polaris"'
    );
    data = data.replace(
        new RegExp('MENU_BAR_POLARIS_LOGO', 'g'),
        '"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAJKSURBVEhLtZXPS9tgGMe/6Q9r01Y7pyiWlWFRmTpXNybOy2AIO6gTBwpWYV4Ezz3sH1AP/gPuphdF1It49bLD8FAnMoa0MBXGJrYeiiFNbU2b+OTHac2yRNYPNHngffJ++zzfJ28YmUAVcej3qmFawfr6d8iyBIYBampcKJeBqaluiGIZbrdTzzLHVIBlP+LmpqjGLpcPXq8DR0dxrK5+xcDAQ4yNvVDXzDBtUTAYoGu9EqGhoQ7NzUESOMfgYAjb2yns75/h5CSt5v4Nyx4wjIxi0UFVsNjZSSIWe47Fxc9oa2tEMnmlZ1Vi2qLW1gVcXooUMQgEnJidjSKXKyGd5mktiJ6eehwe/sTGxgftAQMsV8DzBczPv6L+v8fISCdt/Fv999FohPz4pGdVYmtMeV4zfG7uJdrbH2Bv7weamnxYWnqL21saMQPu9R44nQxKJRkcJ9KUichkBEiScadtCfj9HvW+ufmNNs1hZqYb2WweKytfUFvrUtf+xKbJz5DPSzg9zaKjoxGRiB+pVAZrazHtAQMsV8CybuzunmF0tBPhcB36+0M4OEhjefkdEolfelYllgWUOj0eiXqex/j4E9r8HJOTT3FxwZHYIz2rEssCDB1IHCfQWD7G8fEVhobCmJ7uRV9fSM8wxrIHXi8Qj7+mt/cNBKEIn08z/F9YrkAZx4mJLjW2urmCrTEtFEp6ZB1bAsp3wS6mAtfXAl15/Zej09T4ODDD1OStrQQZqn3RJEnC8HAvWlr8+qo1TAX+B7Y8uA9VFgDuAGLN1z00rPbxAAAAAElFTkSuQmCC"'
    );
    // Add the Materials button 
    if (r.variables.environment !== "prod") {
        data = data.replace(
            'var sMenuBarRight',
            'sMenuBarLeft += \'<td class="menu"><img alt="Launch Materials" border="0" class="clickable" onclick="openMaterials()" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAIAAABvFaqvAAAACXBIWXMAAAsSAAALEgHS3X78AAABYUlEQVQ4jWNsmHaOgRqAiSqmUNMgFgilIMUT768KF9247+GFm++wakBTuXDj7QfPvuB0kbu1jAAvG6Y4Bzuzv5M8Vi3YDeJgZw73UMIU93eUx2oBToMYGBgkRDgdTCWRRQzUhTQU+XGpxxfY9iYSClI8ELYAL5u7tQwexQRizd9JnoOdmYGBIdxDCcLABVgwhV68+S7AywbRJsDL5u8o//LtdwkRTojsj59/P3z+Befic9GPX3837n8I52oo8tubSMC5K3fc+/HrL6Yu7F67cf/jiUuvMcVPXHoNSTXEGsTAwLDz6JMXb74ji7x4833n0Se41OML7JU77v34CfXFj59/V+64h0cxPoM+fP4FD6ydR598+PwLj2IssYYMbtz/CMl0uLIesQYxMDBs3PeQoBqGwVgeDT6DoIH94u33hRtvQ9hYcwAy2Hn0CQcbM1wjikE/fv7FlfYxAVqKhwCqeQ0ADLZ3Ze4C+AkAAAAASUVORK5CYIKJUE5HDQoaCgAAAA1JSERSAAAAWgAAAFoIAgAAALfK1lUAAAAJcEhZcwAACxIAAAsSAdLdfvwAAANWSURBVHic7ZtdtaMwFEbprBEQCUhIHSABHIADqqCgAAngAByAA6iC1gFxkPvADPce2tITyA+z5uznkoTd5DshpScppUf85ZfrARwL0gEgHQDSASAdANIBIB0A0gEgHQDSASAdAJSOrutOaM7ns+lBbxhV13WYNvXPjmEY8jzX3uwCIUSSJNqbNbJYsixDfhubSZLk8Xhob9ZUdiRJIoQw1HhVVU3TmGjZlI7H42FiMk8tXy4XEy17RitL0zQmvsMoiszNO7OFVvsKz/N8GAaNDS4wq0Nv/nddl2WZrtZeYnwb1nWdlrprqLIukQjatt3ZS9/3mI5WCMNwzwDatsX0YmmTvjP/DKXyM5Z07KmO5mr2M/Ye4TbvnYzu6BZYfaLdUHfzPDe93wdgAmZ/lM4EQYCPz77vdfV7rCidwdddIUQURabHs8Ty7JjA1N04jjX2eNDZMfGx7jZNU1WVreF840bHet21WVkXaNbBGEN+cqXu4isrvjsserOjrmt814yxcRwXfSk9pOEHhswOzTqkYgQu6q5SZU3TVKLf1XGmYxxHzjn+roqimHoZx9H3feRVnPM/N4DDmQ4pZd/3Sqt6qrv4acUYu9/v/4wOKWVZlshLPM/jnCt9vizL7xvA4ViH3H1C8Y44jsEN4HCvQykLkHDOF8UIeaH7XSljTKnuYijLUv9e4wdmd6Wc86IodLVWFIVSzdqCucUyoyVEwjB82TjycveLZaYsy50hwhhTKj2bsaFj/83UdW00MmYsPdEGQbD5F6Msy4Ig0DmaFSxkx8yGu/p4mIhs50DZMaM6561FxoxVHao7kf0ZrIrt07AgCNI0xXwyTVND2/w1bGbHzMfd1PNm/B3IgR0xO2bWQ2SKDDuVdYEbHb7vr2Skjc34G5y9ZhuG4csQCcNQ7y8samBWlPbsmHg+RvR9HxkZM8iBHTo7Jp4zwtpm/B2O30n/eQKQZZmryJj57bZ7z/PiOL7dbkKI6/XqeiwH0OF5nsYjop3QHzgApANAOgCkA0A6AKQDQDoApANAOgCkA0A6AKQDQDoApANAOgCkA0A6AKjTMM659v9w6AI5MOQp7Al/Nv8/QIsFQDoApANAOgCkA0A6AKQDQDoApANAOgCkA0A6AKQD8AUbhFh0ECAh+wAAAABJRU5ErkJggg=="></td>\';\n\tvar sMenuBarRight'
        );
    }
    // Add the Task List button 
    data = data.replace(
        'var sMenuBarRight',
        'sMenuBarLeft += \'<td class="menu"><img alt="Launch Task List" border="0" class="clickable" onclick="openTaskList()" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAACg0lEQVRIic2Wzys0cRzHX9+1MxO1HKbIr3a0pbF+bBFNyoGbOOCgHDZOwsHJwcW/4KA4yEVydHBBbiIZcZPnuTlwoZU2dkntfp7Dk83+GE/U1POuz2E+33p95j3vme98VSRsCT4qCHD1+5cv8Da7hYAv5E/6PwY8PT2RzWb9GXB6eopt2+zv7/9oQLCwkUwm2dvbY3x8HNd1GR0dpbu7m4GBAZaWljg6OvKEhcNhNjc3CQQ+3XckbMnr62uujo+PJRgMyvDwsFRWVkpvb6/c399LOp2W/v5+MQzDsxoaGuTl5SXHioQtUZGwJZ9fUxFha2uLubk5HMdhZ2eHqqoqAN7f30mn054ODMOgvLw8d91mtxQ/IqUU8Xic5uZmWltbCYVCuTVd19F13XNAKZUMWSmF4ziEQiESiQQjIyNcXFx8C/yhIgfZbJaHhwdqamp4fHxkaGiIRCKBaZocHBxwdnbmCWtqamJycjK/WRjy4eGhaJomKysr0tXVJaZpyvn5uaRSKbFtWwDP0jRNnp+f80IucuA4DmNjY8zPz2OaJru7u7S3twOwvb3N9fW1p4O6ujqCwXxk0QBN01hfX8e2bQYHB4nFYrm1aDRKNBr1HFBKJUPWdZ3FxUVisRiZTIbV1VXu7u6+Bf5QkQP4+y0opchkMiwsLLCxsUFnZyeGYXBzc+MJq66uxrKs/GZhyK7rSk9Pj1xeXsrMzIwEAgFZW1uTVColHR0dX4ZcUVHx75Bra2tJJpP09fXx9vbG8vIyU1NTAMzOzuK6rqeD+vp6ysrK8npFWwXA7e0tExMTxONxpqenUUp5Qr9Sya0CoLGxkZOTkx9BC+X7H035fapQIr7y+QMKpjOoViG+vQAAAABJRU5ErkJggg=="></td>\';\n\tvar sMenuBarRight'
    );
    return data;
}

function __replaceCmsDomainsGeneric(r, data, flags, host)
{
    // If a 302 has been issued then there's no point in processing in the response body
    if(r.status === 302) {
        r.sendBuffer(data, flags);
        return;
    }

    let cmsEnv = __getCmsEnv(r);

    let replacements = [
        {old: r.variables[cmsEnv + 'UpstreamCmsModernDomainName'], new: host},
        {old: r.variables[cmsEnv + 'UpstreamCmsServicesDomainName'], new: host},
        {old: r.variables[cmsEnv + 'UpstreamCmsDomainName'], new: host},
        {old: r.variables[cmsEnv + 'UpstreamCmsIpCorsham'], new: host},
        {old: r.variables[cmsEnv + 'UpstreamCmsModernIpCorsham'], new: host},
        {old: r.variables[cmsEnv + 'UpstreamCmsIpFarnborough'], new: host},
        {old: r.variables[cmsEnv + 'UpstreamCmsModernIpFarnborough'], new: host}
    ];

    r.sendBuffer(__replaceContent(data, replacements), flags);
}

function __replaceContent(content, replacements)
{
    
    for (let i = 0; i < replacements.length; i++) {
        let reg = /[-=./]/gm;
        let rep = replacements[i];
        let repold = (rep.old).replace(reg, "");
        let regexp = new RegExp(repold, 'g');
        content = content.replace(regexp, rep.new);
    }
    return content;
}

/*
 * Detect the CMS environment from the cookies
 */
function __getCmsEnv(r)
{
    let cookie = r.headersIn.Cookie || '';
    return __getCmsEnvInternal(cookie);
}

function __getCmsEnvCookieOut(r)
{
    let cookies = r.headersOut['Set-Cookie'] || [''];
    return __getCmsEnvInternal(cookies[0]);
}

function __getCmsEnvInternal(cookie)
{
    if(cookie.includes("cin3")) return "default";
    if(cookie.includes("cin2")) return "cin2";
    if(cookie.includes("cin4")) return "cin4";
    if(cookie.includes("cin5")) return "cin5";
    return "default";
}

export default {
    proxyDestinationCorsham, proxyDestinationCorshamInternal, proxyDestinationModernCorsham, proxyDestinationModernCorshamInternal,
    proxyDestinationFarnborough, proxyDestinationFarnboroughInternal, proxyDestinationModernFarnborough, proxyDestinationModernFarnboroughInternal,
    upstreamCmsDomainName, upstreamCmsModernDomainName, replaceCmsDomains, replaceCmsDomainsAjaxViewer, upstreamCmsServicesDomainName,
    cmsMenuBarFilters, upstreamCmsIpCorsham, upstreamCmsIpFarnborough, upstreamCmsModernIpFarnborough, upstreamCmsModernIpCorsham,
    devLoginEnvCookie, getDomainFromCookie
}