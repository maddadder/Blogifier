#sudo mount 192.168.1.65:/myshare /nfs/home

kubectl cp --retries=5 leenet/blogifier-plhhoa-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/plhhoa
kubectl cp --retries=5 leenet/blogifier-zambonigirl-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/zambonigirl
kubectl cp --retries=5 leenet/blogifier-pawsnclaws-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/pawsnclaws
kubectl cp --retries=5 leenet/blogifier-paintedravendesign-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/paintedravendesign
kubectl cp --retries=5 leenet/blogifier-ollie-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/ollie
cp -r /nfs/home ~/Downloads/backups/blogifier/backups
