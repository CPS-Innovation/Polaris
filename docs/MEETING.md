# Polaris flow

- `cms.cps.gov.uk/polaris` plus optional `?r=landing-page-url`
- `polaris.cps.gov.uk/init?cookies=...`
- 302 to get token
- further 302s until user gets taken to `landing-page-url`

# Hiding CMS cookies from apps

- add 302(s) that find the user's Entra id
- stash cookies/modern token
- we put this is the _handover_ flow without asking CGI to do anything
- soon all apps hit MDS with OAuth/OIDC/Entra id and

# ... but a bonus for us to capture fresh logins

- asking CGI to put a JS snippet into the login page success response
- user does not even have to go through `/polaris` handover for us to know the new cookies/token
- fires off the `/polaris` flow in an iframe, we store new cookies against AD identity

```js
// one line
// try{var _f=top.document.createElement("iframe");_f.src="/polaris-v2";_f.style.display="none";_f.onload=function(){_f.parentNode.removeChild(_f);};top.document.documentElement.appendChild(_f);}catch(e){}

// formatted
try {
  var _f = top.document.createElement("iframe");
  _f.src = "/polaris-v2";
  _f.style.display = "none";
  _f.onload = function () {
    _f.parentNode.removeChild(_f);
  };
  top.document.documentElement.appendChild(_f);
} catch (e) {}
```

# Bonus for George

- at the end of the iframe flow we can put the `id-token` into `localStorage`
- Watchdog spec for CGI is to pass `Authorisation: Bearer <id-token>` in HTTP header, just reading from `localStorage`

# Adding an endpoint into MDS to store CMS cookies against Entra Id

- to discuss
