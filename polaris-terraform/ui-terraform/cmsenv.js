function proxyDestinationCorsham(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsIpCorsham'];
}

function proxyDestinationCorshamInternal(r)
{
    return proxyDestinationCorsham(r) + '/';
}

function proxyDestinationModernCorsham(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsModernIpCorsham'];
}

function proxyDestinationModernCorshamInternal(r)
{
    return proxyDestinationModernCorsham(r) + '/';
}

function proxyDestinationFarnborough(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsIpFarnborough'];
}

function proxyDestinationFarnboroughInternal(r)
{
    return proxyDestinationFarnborough(r) + '/';
}

function proxyDestinationModernFarnborough(r)
{
    let cmsEnv = __getCmsEnv(r);
    return r.variables.endpointHttpProtocol + '://' + r.variables[cmsEnv + 'UpstreamCmsModernIpFarnborough'];
}

function proxyDestinationModernFarnboroughInternal(r)
{
    return proxyDestinationModernFarnborough(r) + '/';
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

function replaceCmsDomains(r, data, flags)
{
    __replaceCmsDomainsGeneric(r, data, flags, r.variables.host);
}

function replaceCmsDomainsAjaxViewer(r, data, flags)
{
    __replaceCmsDomainsGeneric(r, data, flags, r.variables.websiteHostname);
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
        {old: r.variables[cmsEnv + 'UpstreamCmsModernIpFarnborough'], new: host},
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
 * Detect the CMS environment from the specific entry cookie, or by looking for an existing BIG-IP cookie
 * from an environment.
 */
function __getCmsEnv(r)
{
    let cookie = r.headersIn.Cookie || '';
    let cmsEnv = __getCookie(cookie, '__CMSENV') || '';

    if(!['default','cin4','cin5'].includes(cmsEnv)) {
        if(cookie.includes("cin3")) return "default";
        if(cookie.includes("cin4")) return "cin4";
        if(cookie.includes("cin5")) return "cin5";
        return "default";
    }
    
    return cmsEnv;
}

function __getCookie(cookieHeader, name)
{
    let match = cookieHeader.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
    return match ? match[1] : null;
}

export default { 
    proxyDestinationCorsham, proxyDestinationCorshamInternal, proxyDestinationModernCorsham, proxyDestinationModernCorshamInternal, 
    proxyDestinationFarnborough, proxyDestinationFarnboroughInternal, proxyDestinationModernFarnborough, proxyDestinationModernFarnboroughInternal, 
    upstreamCmsDomainName, upstreamCmsModernDomainName, replaceCmsDomains, replaceCmsDomainsAjaxViewer 
}