docker-compose build
docker push neon-registry.b4c5-2917-0fd3-88a5.neoncluster.io/leenet/postgres-backup:latest
kubectl apply -f deployment.yaml --namespace leenet
