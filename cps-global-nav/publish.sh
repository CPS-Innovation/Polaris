ENVIRONMENT=$1
VERSION=$(jq -r '.version' package.json)

rm -r ./publish/*
mkdir -p ./publish/$ENVIRONMENT

npm run build

cp -R ./dist/* ./publish/$ENVIRONMENT
azcopy sync ./publish/$ENVIRONMENT https://sacpsglobalnavpoc.blob.core.windows.net/$ENVIRONMENT/ --delete-destination=true

#azcopy copy "./publish/$ENVIRONMENT/*" https://sacpsglobalnavpoc.blob.core.windows.net/versions/$VERSION --overwrite=false --recursive
