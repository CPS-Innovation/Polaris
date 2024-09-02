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

function environmentCookies(r)
{
    let cookieEnv = r.variables.cookieEnv;
    r.headersOut['Set-Cookie'] = '__CMSENV=' + cookieEnv;
    
    if(cookieEnv != 'default') {
        r.headersOut['Set-Cookie'] = 'BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
        r.headersOut['Set-Cookie'] = 'BIGipServer~ent-s221~CPSAFP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    }
    if(cookieEnv != 'cin4') {
        r.headersOut['Set-Cookie'] = 'BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN4-cin4.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
        r.headersOut['Set-Cookie'] = 'BIGipServer~ent-s221~CPSAFP-LTM-CM-WAN-CIN4-cin4.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    }
    if(cookieEnv != 'cin5') {
        r.headersOut['Set-Cookie'] = 'BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN5-cin5.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
        r.headersOut['Set-Cookie'] = 'BIGipServer~ent-s221~CPSAFP-LTM-CM-WAN-CIN5-cin5.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    }
}

function __addAppLaunchButtonsToMenuBar(r, data, flags)
{
    data = data.replace(new RegExp('objMainWindow\.top\.frameData\.objMasterWindow\.top\.frameServerJS\.POLARIS_URL', 'g'), '"/polaris"');
    data = data.replace(new RegExp('MENU_BAR_POLARIS_LOGO', 'g'), '"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAJKSURBVEhLtZXPS9tgGMe/6Q9r01Y7pyiWlWFRmTpXNybOy2AIO6gTBwpWYV4Ezz3sH1AP/gPuphdF1It49bLD8FAnMoa0MBXGJrYeiiFNbU2b+OTHac2yRNYPNHngffJ++zzfJ28YmUAVcej3qmFawfr6d8iyBIYBampcKJeBqaluiGIZbrdTzzLHVIBlP+LmpqjGLpcPXq8DR0dxrK5+xcDAQ4yNvVDXzDBtUTAYoGu9EqGhoQ7NzUESOMfgYAjb2yns75/h5CSt5v4Nyx4wjIxi0UFVsNjZSSIWe47Fxc9oa2tEMnmlZ1Vi2qLW1gVcXooUMQgEnJidjSKXKyGd5mktiJ6eehwe/sTGxgftAQMsV8DzBczPv6L+v8fISCdt/Fv999FohPz4pGdVYmtMeV4zfG7uJdrbH2Bv7weamnxYWnqL21saMQPu9R44nQxKJRkcJ9KUichkBEiScadtCfj9HvW+ufmNNs1hZqYb2WweKytfUFvrUtf+xKbJz5DPSzg9zaKjoxGRiB+pVAZrazHtAQMsV8CybuzunmF0tBPhcB36+0M4OEhjefkdEolfelYllgWUOj0eiXqex/j4E9r8HJOTT3FxwZHYIz2rEssCDB1IHCfQWD7G8fEVhobCmJ7uRV9fSM8wxrIHXi8Qj7+mt/cNBKEIn08z/F9YrkAZx4mJLjW2urmCrTEtFEp6ZB1bAsp3wS6mAtfXAl15/Zej09T4ODDD1OStrQQZqn3RJEnC8HAvWlr8+qo1TAX+B7Y8uA9VFgDuAGLN1z00rPbxAAAAAElFTkSuQmCC"');
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
        let rep = replacements[i];
        content = content.replace(new RegExp(rep.old, 'g'), rep.new);
    }
    return content;
}

/*
 * Detect the CMS environment from the cookies
 */
function __getCmsEnv(r)
{
    let cookie = r.headersIn.Cookie || '';

    if(cookie.includes("cin3")) return "default";
    if(cookie.includes("cin4")) return "cin4";
    if(cookie.includes("cin5")) return "cin5";
    return "default";
}

export default {
    proxyDestinationCorsham, proxyDestinationCorshamInternal, proxyDestinationModernCorsham, proxyDestinationModernCorshamInternal,
    proxyDestinationFarnborough, proxyDestinationFarnboroughInternal, proxyDestinationModernFarnborough, proxyDestinationModernFarnboroughInternal,
    upstreamCmsDomainName, upstreamCmsModernDomainName, replaceCmsDomains, replaceCmsDomainsAjaxViewer, upstreamCmsServicesDomainName,
    cmsMenuBarFilters, environmentCookies
}