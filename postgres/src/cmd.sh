#!/bin/sh
logfile="/mnt/nfs/pgsql.log"
backup_dir="/mnt/nfs"
touch $logfile
databases=`psql -h $PGHOST -U postgres -q -c "\l" | sed -n 4,/\eof/p | grep -v rows\) | grep -v template0 | grep -v template1 | awk {'print $1'}`
echo "writing to log file"
echo "Starting backup of databases " >> $logfile
for i in $databases; do
    if [ ${#i} -ge 3 ]; then
        dateinfo=`date '+%Y-%m-%d %H:%M:%S'`
        timeslot=`date '+%Y%m%d%H%M'`
        /usr/bin/vacuumdb -z -h $PGHOST -U postgres $i >/dev/null 2>&1
        /usr/bin/pg_dump -U postgres -F c -b $i -h $PGHOST -f $backup_dir/$i-database-$timeslot.backup
        echo "Backup and Vacuum complete on $dateinfo for database: $i " >> $logfile
    fi
    
done
echo "Done backup of databases " >> $logfile