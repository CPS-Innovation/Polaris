using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Common.Constants;

namespace PolarisGateway.Tests.Functions.SharedMethods
{

    public class SharedMethods
    {
        protected static HttpRequest CreateHttpRequest()
        {
            const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new KeyValuePair<string, StringValues>(OAuthSettings.Authorization, token));
            context.Request.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
            context.Request.Headers.Add("Cookie", new CookieHeaderValue("cms-auth-values", "some-string").ToString());
            context.Request.QueryString = new QueryString("?query=defendant");
            return context.Request;
        }

        protected static HttpRequest CreateHttpRequest(object requestBody)
        {
            const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestBody)));
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new KeyValuePair<string, StringValues>(OAuthSettings.Authorization, token));
            context.Request.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
            context.Request.Headers.Add("Cookie", new CookieHeaderValue("cms-auth-values", "some-string").ToString());
            context.Request.Body = stream;
            context.Request.ContentLength = stream.Length;
            context.Request.QueryString = new QueryString("?query=defendant");
            return context.Request;
        }

        protected static HttpRequest CreateHttpRequestWithoutToken()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
            context.Request.Headers.Add("Cookie", new CookieHeaderValue("cms-auth-values", "some-string").ToString());
            context.Request.QueryString = new QueryString("?query=defendant");
            return context.Request;
        }

        protected static HttpRequest CreateHttpRequestWithoutCorrelationId()
        {
            const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new KeyValuePair<string, StringValues>(OAuthSettings.Authorization, token));
            context.Request.Headers.Add("Cookie", new CookieHeaderValue("cms-auth-values", "some-string").ToString());
            context.Request.QueryString = new QueryString("?query=defendant");
            return context.Request;
        }

        protected static HttpRequest CreateHttpRequestWithoutCmsAuthValuesToken()
        {
            const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(new KeyValuePair<string, StringValues>(OAuthSettings.Authorization, token));
            context.Request.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
            context.Request.QueryString = new QueryString("?query=defendant");
            return context.Request;
        }
    }
}
