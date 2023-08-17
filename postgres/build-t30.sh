docker-compose build
docker push 192.168.1.151:32000/postgres-backup:latest
kubectl apply -f deployment-t30.yaml --namespace default
