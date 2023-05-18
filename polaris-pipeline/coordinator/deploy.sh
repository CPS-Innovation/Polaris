rm -rf publish
rm publish.zip
dotnet clean
dotnet publish -o publish -c Release
(cd publish && zip -r ../publish.zip *)  
az functionapp deployment source config-zip --src publish.zip -n stef-throwaway-coordinator -g rg-polaris-dev