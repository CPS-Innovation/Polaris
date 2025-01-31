# Deployed file structure

```
sacpsglobalnavpoc.blob.core.windows.net/
    index.html - guidance etc
    dummy-home-page/
        dev/
            index.html
        qa/
            index.html
        prod/
            index.html
    latest/
        cps-global-nav/
        etc...
    dev/
        cps-global-nav/
        etc...
    qa/
        cps-global-nav/
        etc...
    prod/
        cps-global-nav/
        etc...
    versions/
        0.1.0/
            cps-global-nav/
            etc...
```

# Todo

- Permissions to deploy

  - Azure IAM on storage account

- Cosmetic assumptions

  - hide second row if not case context

- Difficult to completely key via address as there may be different contexts for the same app (development machine; deployed QA)

- Semantic/accessible HTML

# Config!

```json
{
  "dev": {
    "level1": {
      "home": {
        "label": "Home",
        "appType": "notOutSystems",
        "pageIdentifierRegex": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html",
        "inboundUrl": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html"
      },
      "tasks": {
        "label": "Tasks",
        "appType": "outSystems",
        "pageIdentifierRegex": "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/TaskList",
        "inboundUrl": "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/TaskList"
      },
      "cases": {
        "label": "Cases",
        "appType": "notOutSystems",
        "pageIdentifierRegex": "https://polaris-dev-cmsproxy.azurewebsites.net/polaris-ui/case-search",
        "inboundUrl": "https://polaris-dev-cmsproxy.azurewebsites.net/polaris-ui/case-search"
      }
    },
    "level2": {
      "details": {
        "label": "Details",
        "appType": "notOutSystems",
        "pageIdentifierRegex": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html",
        "inboundUrl": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html"
      },
      "caseMaterials": {
        "label": "Materials",
        "appType": "notOutSystems",
        "pageIdentifierRegex": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html",
        "inboundUrl": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html"
      },
      "review": {
        "label": "Details",
        "appType": "notOutSystems",
        "pageIdentifierRegex": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html",
        "inboundUrl": "https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/latest/index.html"
      }
    }
  }
}
```
