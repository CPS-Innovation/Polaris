namespace PolarisAuthHandover.Domain.Enums
{
    public enum AuthFlowMode
    {
        // There are two mechanisms for redirecting back to the client.  
        //  Mechanism 1: the Polaris UI has detected missing or expired auth and redirects to this endpoint with
        //    a query param that contains the URL to redirect to after auth.
        PolarisAuthRedirect,
        // Mechanism 2: we are brought here from CMS.  The scheme there is that we are passed a case id of a case.
        //  This is passed in the form of `q=%7B%22caseId%22%3A2073383%7D` where there is a fragment of JSON that
        //  contains the case id.  We need to extract this and then call Modern to get the URN to form our full
        //  redirect URL.
        CmsLaunch
    }
}