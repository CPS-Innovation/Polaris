This folder includes copies of the nginx resources that are used to spin up the polaris proxy using an empty app service (so nginx deployed to a custom VM in azure).
These resources are used to test the njs functions only so far, using postman pointed to a manully added "test" endpoint defined in the nginx.conf file.

The basic commands to get this custom nginx image built and up and running in your local Docker app:

1. > docker build -t polarisproxyimage .
2. > docker run --name polarisproxy -p 81:80 -d polarisproxyimage

where "81" is a custom port and can be whatever suits your local environment

You will see that the dockerfile includes some steps that add hardcoded environment variables so that the njs script is exercised more faithfully.
It also overrides the standard nginx conf file with this locally amended version. I've manually overwritten other environment variables that are normally injected into the .conf file by the app settings in the running app service.
These can also be injected into the test conf file by adding more environment variables in the dockerfile and entered into the relevant parts of the test conf file definition. 
Click on the running instance inside your local docker application to view the logs. You can write your own entries (as you will see in the test nginx.js test file) by using "r.log('message')" statements to expose your script flow.

Note: To deploy the resulting nginx image to your local Docker app you will need the appropriate VPN switched on.