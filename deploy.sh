docker-compose build
docker push neon-registry.b4c5-2917-0fd3-88a5.neoncluster.io/leenet/blogifier:1.12.51

#plhhoa
helm upgrade blogifier-plhhoa -f ./chart/values.yaml -f ./chart/values.plhhoa.yaml ./chart --namespace leenet

#zambonigirl
helm upgrade blogifier-zambonigirl -f ./chart/values.yaml -f ./chart/values.zambonigirl.yaml ./chart --namespace leenet

#paintedravendesign
helm upgrade blogifier-paintedravendesign -f ./chart/values.yaml -f ./chart/values.paintedravendesign.yaml ./chart --namespace leenet

#pawsnclaws
helm upgrade blogifier-pawsnclaws -f ./chart/values.yaml -f ./chart/values.pawsnclaws.yaml ./chart --namespace leenet

#ollie
helm upgrade blogifier-ollie -f ./chart/values.yaml -f ./chart/values.ollie.yaml ./chart --namespace leenet

#leenet
helm upgrade blogifier-leenet -f ./chart/values.yaml -f ./chart/values.leenet.yaml ./chart --namespace leenet

#whereatreeoncewas
helm upgrade blogifier-whereatreeoncewas -f ./chart/values.yaml -f ./chart/values.whereatreeoncewas.yaml ./chart --namespace leenet
