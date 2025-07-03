## 1. enter the container
```
docker exec -it splunk bash
```
## 2. Run this 

```
sudo tee /opt/splunk/etc/system/local/server.conf > /dev/null <<EOF
[httpServer]
enableSplunkWebSSL = true
sslKeysfile = /certs/splunk.dockube.lan.key
sslCertfile = /certs/splunk.dockube.lan.pem
EOF
```