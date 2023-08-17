#sudo mount 192.168.1.65:/myshare /nfs/home

kubectl cp --retries=5 default/blogifier-plhhoa-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/plhhoa
kubectl cp --retries=5 default/blogifier-zambonigirl-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/zambonigirl
kubectl cp --retries=5 default/blogifier-pawsnclaws-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/pawsnclaws
kubectl cp --retries=5 default/blogifier-paintedravendesign-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/paintedravendesign
kubectl cp --retries=5 default/blogifier-ollie-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/ollie
kubectl cp --retries=5 default/blogifier-whereatreeoncewas-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/whereatreeoncewas
cp -r /nfs/home ~/Downloads/backups/blogifier/backups
