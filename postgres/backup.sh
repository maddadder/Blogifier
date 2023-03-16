#sudo mount 192.168.1.65:/myshare /nfs/home

kubectl cp leenet/blogifier-plhhoa-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/plhhoa
kubectl cp leenet/blogifier-zambonigirl-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/zambonigirl
kubectl cp leenet/blogifier-pawsnclaws-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/pawsnclaws
kubectl cp leenet/blogifier-paintedravendesign-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/paintedravendesign
kubectl cp leenet/blogifier-ollie-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/ollie
cp -r /nfs/home ~/Downloads/backups/blogifier/backups
