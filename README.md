# Homelab dashboard
Project to create a dashboard, tracking and monitoring solution for homelab users. \

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
