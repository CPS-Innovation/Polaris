echo '==== Add Microsoft package source ===='
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i ./packages-microsoft-prod.deb
rm ./packages-microsoft-prod.deb
sudo apt-get update

echo '==== Update from package sources ===='
sudo apt-get upgrade -y

echo '==== Install dependencies ===='
DEBIAN_FRONTEND=noninteractive TZ=Etc/UTC apt -y install tzdata

echo '==== Cleaning up inherited package sources first ===='
sudo apt-get remove 'dotnet*'
sudo apt-get remove 'aspnetcore*'
sudo rm /etc/apt/sources.list.d/microsoft-prod.list
sudo apt-get update

echo '==== dotnet 3.1 ===='
# dotnet 3.1 is used by azure devops itself
sudo apt-get update && sudo apt-get install -y dotnet-sdk-3.1

echo '==== dotnet 6 ===='
sudo apt-get update && sudo apt-get install -y dotnet-sdk-6.0

echo '==== dotnet 7 ===='
sudo apt-get update && sudo apt-get install -y dotnet-sdk-7.0

echo '==== PowerShell ===='
sudo apt-get update && apt install -y powershell

echo '==== Utils ===='
sudo apt-get update -yq
sudo apt-get install -y wget 
sudo apt-get install -y apt-utils 
sudo apt-get install -y apt-transport-https 
sudo apt-get install -y git
sudo apt-get install -y ca-certificates
sudo apt-get install -y curl
sudo apt-get install -y iputils-ping
sudo apt-get install -y jq
sudo apt-get install -y lsb-release
sudo apt-get install -y software-properties-common
sudo apt-get install -y zip
sudo apt-get install -y unzip 
sudo apt-get clean

echo '==== Azure CLI ===='
curl -sL https://aka.ms/InstallAzureCLIDeb | bash

sudo apt-get install libc6
sudo apt-get install libgcc1
sudo apt-get install libgcc-s1
sudo apt-get install libgssapi-krb5-2
sudo apt-get install libicu70
sudo apt-get install liblttng-ust1
sudo apt-get install libssl3
sudo apt-get install libstdc++6
sudo apt-get install libunwind8
sudo apt-get install zlib1g

echo '==== Node.js, related utils and Python3 ===='
sudo apt-get update -yq
sudo apt-get -yq install curl gnupg ca-certificates
curl -L https://deb.nodesource.com/setup_18.x | bash
sudo apt-get install -y npm 
sudo apt-get install -y nodejs
sudo apt-get install -y build-essential
sudo apt-get install -y libgtk2.0-0
sudo apt-get install -y libgtk-3-0
sudo apt-get install -y libgbm-dev
sudo apt-get install -y libnotify-dev
sudo apt-get install -y libgconf-2-4
sudo apt-get install -y libnss3
sudo apt-get install -y libxss1
sudo apt-get install -y libasound2
sudo apt-get install -y libxtst6
sudo apt-get install -y xauth
sudo apt-get install -y xvfb
sudo apt-get install -y python3-pip
sudo apt-get clean

echo '==== Installing NuGet via Mono and Microsoft.ApplicationInsights ===='
sudo apt-get install -y mono-complete
sudo wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -O /usr/local/bin/nuget
sudo chmod +x /usr/local/bin/nuget
sudo mono nuget install Microsoft.ApplicationInsights
sudo apt-get clean