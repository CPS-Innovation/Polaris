ENVIRONMENT=$1


rm -rf ./publish/*
mkdir -p ./publish

# Copy everything that is not index.html...
shopt -s extglob
cp -R ./dummy-home-page/!(index.html) ./publish
# ... because we will be copying it as we replace text
sed "s/%env%/$ENVIRONMENT/g" ./dummy-home-page/index.html > ./publish/index.html

azcopy sync ./publish https://sacpsglobalnavpoc.blob.core.windows.net/dummy-home-page/$ENVIRONMENT/ --delete-destination=true

