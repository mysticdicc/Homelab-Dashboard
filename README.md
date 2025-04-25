# Homelab dashboard
Project to create a dashboard, tracking and monitoring solution for homelab users.

Basic functionality:
1. A small dashboard where you can save links with iconss.
2. A subnet tracker that lets you save a list of IP addresses and hostnames, useful if you have a large number of network devices.
3. Monitoring functionality and reporting.

To run download and run the docker container:

<pre>
docker run -d \
  --name=homelab-dashboard \
  -p 8080:8080 \
  --restart unless-stopped \
  mysticdicc/homelab-dashboard:latest
</pre>
  
To run with persistent config between updates:

<pre>
docker run -d \
  --name=homelab-dashboard \
  -p 8080:8080 \
  --restart unless-stopped \
  -v /local/path/for/config:/app/data \
  mysticdicc/homelab-dashboard:latest
</pre>

Once running you can access the website at http://dockerip:8080 by default, if you have changed the port you will need to reflect this in the URL.

# Dashboard
Has the ability to add new and edit existing links saved previously. Clicking on the cards with the edit feature shut will take you to the links making it easier to keep track of lots of URLs which is quite a common issue if you host a lot of docker containers such as the ARR stacks.

![dashboard_home](/gitimages/dashboardlinks.png)

# Subnet Tracker
Generates a list of all IP addresses in a subnet if you provide the IP range in CIDR notation in the top left box. You can then expand the subnets and mark down names against them and enable monitoring for the background monitoring service.

Pressing the "refresh" icon on the subnet row will start a discovery task, any devices that respond to ICMP (ping) will automatically be added to monitoring, it will also attempt to resolve the dns names for these and store them in teh hostname field.

![subnet_home_open](/gitimages/subnettrackeropened.png)
![subnet_home_closed](/gitimages/subnettrackerclosed.png)

# Monitoring
Shows you a line chart which shows the various monitoring states for all of your monitored devices over time, with the ability to change the polling interval for the monitoring service and change the scale of the graph to different time scales.

![monitoring_home](/gitimages/monitoring.png)
![monitoring_home_open](/gitimages/monitoringopen.png)
![monitoring_popup_open](/gitimages/monitoring1device.png)

Available under the [GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html#license-text) license
