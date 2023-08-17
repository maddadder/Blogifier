#sudo mount 192.168.1.65:/myshare /nfs/home

kubectl cp ~/Downloads/backups/blogifier/plhhoa default/blogifier-plhhoa-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/zambonigirl default/blogifier-zambonigirl-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/pawsnclaws default/blogifier-pawsnclaws-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/paintedravendesign default/blogifier-paintedravendesign-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/ollie default/blogifier-ollie-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/leenet default/blogifier-leenet-0:/opt/blogifier/outputs/wwwroot/data

# then move the contents from the folder into it's parent, e.g. cd plhhoa; mv ./1 ../

