### Configure postgres backup

0. Install nfs-common on every Kubernetes node `apt-get install -y nfs-common`
1. Go to the deployment-t30.yaml file and update the `PGPASSWORD` environment variable with the `postgres` password.
2. Update the server from `192.168.1.65` to the correct ip address of your nfs server. For the server you can use a raspberry pi running OpenMediaVault
3. Update the path from `/myshare` to the correct nfs file share path
4. run `./build.sh` which will also deploy this backup solution

### To backup the files

1. Run the following manually from your client machine every time you want to backup the files
2. TODO: Please automate this process
```
# backup the files from each website onto local PC
kubectl cp default/blogifier-plhhoa-0:/opt/blogifier/outputs/wwwroot/data ~/Downloads/backups/blogifier/plhhoa
...
See backup-t30.sh
# Backup from from NTF Server onto local PC
sudo mount 192.168.1.65:/myshare /nfs/home
# Check that the volumne mounted ^
cp -r /nfs/home ~/Downloads/backups/blogifier/backups

```
<br/><br/>
### Restore

1. Run in a console `while true; do kubectl port-forward --namespace default pod/acid-minimal-cluster-0 5432:5432; done`
2. to restore db use pg-admin4. To restore make sure to select the role name postgres
3. See `restore.sh`
