#sudo mount 192.168.1.65:/myshare /nfs/home

kubectl cp ~/Downloads/backups/blogifier/plhhoa leenet/blogifier-plhhoa-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/zambonigirl leenet/blogifier-zambonigirl-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/pawsnclaws leenet/blogifier-pawsnclaws-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/paintedravendesign leenet/blogifier-paintedravendesign-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/ollie leenet/blogifier-ollie-0:/opt/blogifier/outputs/wwwroot/data
kubectl cp ~/Downloads/backups/blogifier/leenet leenet/blogifier-leenet-0:/opt/blogifier/outputs/wwwroot/data

# then move the contents from the folder into it's parent, e.g. cd plhhoa; mv ./1 ../

