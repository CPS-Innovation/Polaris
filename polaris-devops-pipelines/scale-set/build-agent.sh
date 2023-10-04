echo '==== Add Microsoft package source ===='
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i ./packages-microsoft-prod.deb
rm ./packages-microsoft-prod.deb
sudo apt update

echo '==== Update from package sources ===='
sudo apt upgrade -y

echo '==== Install dependencies ===='
DEBIAN_FRONTEND=noninteractive TZ=Etc/UTC apt -y install tzdata

echo '==== Cleaning up inherited package sources first ===='
sudo apt remove 'dotnet*'
sudo apt remove 'aspnetcore*'
sudo rm /etc/apt/sources.list.d/microsoft-prod.list
sudo apt update

echo '==== dotnet 3.1 ===='
# dotnet 3.1 is used by azure devops itself
sudo apt update && sudo apt install -y dotnet-sdk-3.1

echo '==== dotnet 6 ===='
sudo apt update && sudo apt install -y dotnet-sdk-6.0

echo '==== dotnet 7 ===='
sudo apt update && sudo apt install -y dotnet-sdk-7.0

echo '==== PowerShell ===='
sudo apt update && apt install -y powershell

echo '==== Utils ===='
sudo apt update -yq
sudo apt install -y wget 
sudo apt install -y apt-utils 
sudo apt install -y apt-transport-https 
sudo apt install -y git
sudo apt install -y ca-certificates
sudo apt install -y curl
sudo apt install -y iputils-ping
sudo apt install -y jq
sudo apt install -y lsb-release
sudo apt install -y software-properties-common
sudo apt install -y zip
sudo apt install -y unzip 
sudo apt clean

echo '==== Azure CLI ===='
curl -sL https://aka.ms/InstallAzureCLIDeb | bash

sudo apt install libc6
sudo apt install libgcc1
sudo apt install libgcc-s1
sudo apt install libgssapi-krb5-2
sudo apt install libicu70
sudo apt install liblttng-ust1
sudo apt install libssl3
sudo apt install libstdc++6
sudo apt install libunwind8
sudo apt install zlib1g

echo '==== Node.js, related utils and Python3 ===='
sudo apt update -yq
sudo apt -yq install curl gnupg ca-certificates
curl -L https://deb.nodesource.com/setup_18.x | bash
sudo apt install -y npm 
sudo apt install -y nodejs
sudo apt install -y build-essential
sudo apt install -y libgtk2.0-0
sudo apt install -y libgtk-3-0
sudo apt install -y libgbm-dev
sudo apt install -y libnotify-dev
sudo apt install -y libgconf-2-4
sudo apt install -y libnss3
sudo apt install -y libxss1
sudo apt install -y libasound2
sudo apt install -y libxtst6
sudo apt install -y xauth
sudo apt install -y xvfb
sudo apt install -y python3-pip
sudo apt clean

sudo yum install -y mono-complete
sudo wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -O /usr/local/bin/nuget
sudo chmod +x /usr/local/bin/nuget
nuget install Microsoft.ApplicationInsights
sudo apt clean