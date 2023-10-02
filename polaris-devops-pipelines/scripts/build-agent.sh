echo '==== Add Microsoft package source ===='
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i ./packages-microsoft-prod.deb
rm ./packages-microsoft-prod.deb
apt-get update

echo '==== Update from package sources ===='
apt-get upgrade -y

echo '==== Install dependencies ===='
DEBIAN_FRONTEND=noninteractive TZ=Etc/UTC apt-get -y install tzdata
apt-get install -y wget apt-utils apt-transport-https software-properties-common git ca-certificates curl iputils-ping jq lsb-release zip unzip
apt-get clean

echo '==== PowerShell ===='
apt-get update && apt-get install -y powershell

echo '==== Azure CLI ===='
curl -sL https://aka.ms/InstallAzureCLIDeb | bash

echo '==== dotnet 3.1 ===='
# dotnet 3.1 is used by azure devops itself
apt-get install -y dotnet-sdk-3.1

echo '==== dotnet 6 ===='
apt-get install -y dotnet-sdk-6.0

echo '==== dotnet 7 ===='
apt-get install -y dotnet-sdk-7.0

echo '==== Node.js and Python3 ===='
curl -sL https://deb.nodesource.com/setup_18.x | bash
apt-get install -y npm
apt-get install -y nodejs
apt-get install -y build-essential
apt-get install -y python3-pip
apt-get clean