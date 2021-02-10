Test locally

```
dotnet fake build --target run
```

Test locally in docker

```
dotnet fake build --target createDockerImage
docker run -it -p 8085:8085 evelina/away-day
```

Push to cloud and deploy

```
dotnet fake build --target pushDockerImage
```

Ssh into web app

This doesn't work
```
https://hut23-away-day.scm.azurewebsites.net/webssh/host
```

TODO: test this one
```
az webapp create-remote-connection --subscription e4662ac5-6762-4c63-b904-e1451b67992c --resource-group hut23-away-day -n hut23-away-day &

```


Ssh into locally running docker container
```
sudo docker ps
```
Find NAME and then run shell inside the container, Alpine linux doesn't have bash
```
sudo docker exec -it NAME /bin/sh
```
