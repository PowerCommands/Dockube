## 1. enter the container
```
docker exec -it splunk bash
```
## 2. Run this

```
sudo chown -R splunk:splunk /opt/splunk
```
## 3. Run this 
```
sudo tee /opt/splunk/etc/system/local/server.conf > /dev/null <<EOF
[httpServer]
enableSplunkWebSSL = true
sslKeysfile = /certs/splunk.dockube.lan.key
sslCertfile = /certs/splunk.dockube.lan.pem
EOF
```

## 4. Verify

```
cat /opt/splunk/etc/system/local/server.conf
```

## 5. Restart Splunk
```
/opt/splunk/bin/splunk restart
```

## 6. Change admin password
```
splunk edit user admin -password <new-password> -auth admin:changeme
```

## 7. Add splunk forward server

```
splunk add forward-server localhost:9997 -auth admin:<new-password>
```