#wait for the SQL Server to come up
echo "waiting for database"
sleep 120s

until /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_Password123 -d master -l 1 -Q "SELECT 1"; do
  echo "Waiting for SQL Server to become available..."
  sleep 1
done

sleep 60s
echo "running set up script"
#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_Password123 -d master -i /src/setup.sql