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

echo '==== dotnet 6 ===='
sudo apt-get update -y && sudo apt-get install -y dotnet-sdk-6.0

echo '==== dotnet 7 ===='
sudo apt-get update -y && sudo apt-get install -y dotnet-sdk-7.0

echo '==== PowerShell ===='
sudo snap install powershell --classic

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
sudo apt-get -yq install ca-certificates curl gnupg
curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | sudo gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_18.x nodistro main" | sudo tee /etc/apt/sources.list.d/nodesource.list
sudo apt-get update && sudo apt-get install nodejs -y
sudo apt-get install -y build-essential
sudo apt-get install -y gcc g++ make
curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | gpg --dearmor | sudo tee /usr/share/keyrings/yarnkey.gpg >/dev/null
echo "deb [signed-by=/usr/share/keyrings/yarnkey.gpg] https://dl.yarnpkg.com/debian stable main" | sudo tee /etc/apt/sources.list.d/yarn.list
sudo apt-get update && sudo apt-get install yarn
echo '==== Install Cypress ===='
sudo npm install cypress --save-dev
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
sudo apt-get clean

echo '==== Install Python, Pip and Checkov'
sudo apt-get update -yq
sudo apt-get install -y python3-pip
sudo pip3 install -U checkov

echo '==== Install Cobertura ===='
sudo apt-get update -yq
sudo apt-get -y install cobertura

echo '==== Install Mono ===='
sudo apt-get update -yq
sudo apt-get install -y mono-complete
echo '==== Install and Run NuGet to add Microsoft.ApplicationInsights ===='
sudo curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
sudo chmod +x /usr/local/bin/nuget.exe
sudo mono /usr/local/bin/nuget.exe install Microsoft.ApplicationInsights 
sudo apt-get clean

echo '==== Install Terraform ===='
sudo apt-get update -yq
sudo apt-get upgrade -y
wget -O- https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
echo "deb [signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
sudo apt-get update -yq && sudo apt-get install -y terraform 
sudo apt-get clean

echo "==== Giving AzDevOps user access to the '/home', '/usr/share', '/usr/bin', and '/opt' directories ===="
sudo chmod -R 777 /home
sudo chmod -R 777 /usr/share
sudo chmod -R 777 /opt

echo "==== Install Chrome ===="
wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | sudo gpg --dearmour -o /usr/share/keyrings/chrome-keyring.gpg
sudo sh -c 'echo "deb [arch=amd64 signed-by=/usr/share/keyrings/chrome-keyring.gpg] http://dl.google.com/linux/chrome/deb/ stable main" > /etc/apt/sources.list.d/google.list'
sudo apt-get update -yq
sudo apt-get install -y google-chrome-stable
sudo apt-get clean