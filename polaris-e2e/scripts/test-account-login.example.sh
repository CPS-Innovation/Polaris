curl https://login.microsoftonline.com/00dd0d1d-d7e6-4338-ac51-565339c7088c/oauth2/v2.0/token \
-d "scope=openid profile https://CPSGOVUK.onmicrosoft.com/fa-rumpole-dev-gateway/user_impersonation" \
-d "grant_type=password" \
-d "username=AutomationUser.ServiceTeam2@cps.gov.uk" \
-d "password=<REDACTED-ASK-SOMEONE_WHO-KNOWS-IT>" \
-d "client_id=a06ec1b9-5154-4f4f-8e15-193bb7c6b971" \
-d "client_secret=<REDACTED-LOOK-IT-UP-IN-AZURE-PORTAL>" 