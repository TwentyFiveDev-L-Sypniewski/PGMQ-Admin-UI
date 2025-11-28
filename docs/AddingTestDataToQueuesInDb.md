# Adding Test Data to Queues (Local Aspire)

## Start Aspire (if not running)

```bash
cd PgmqAdminUI.AppHost
dotnet run
```

## Get Connection Info

```bash
# Find the running PostgreSQL container port
docker ps --filter "name=pgmq-postgres" --format "{{.Ports}}"

# Get the password
docker inspect pgmq-postgres-pgmq-admin-ui --format '{{range .Config.Env}}{{println .}}{{end}}' | grep POSTGRES_PASSWORD
```

## Create a Test Queue

```bash
docker exec -e PGPASSWORD='<password>' pgmq-postgres-pgmq-admin-ui \
  psql -U postgres -d pgmq -c "SELECT pgmq.create('my_test_queue');"
```

## Add Messages

**Simple JSON:**

```bash
docker exec -e PGPASSWORD='<password>' pgmq-postgres-pgmq-admin-ui \
  psql -U postgres -d pgmq -c \
  "SELECT pgmq.send('my_test_queue', '{\"name\": \"John\", \"email\": \"john@example.com\"}');"
```

**Nested JSON:**

```bash
docker exec -e PGPASSWORD='<password>' pgmq-postgres-pgmq-admin-ui \
  psql -U postgres -d pgmq -c \
  "SELECT pgmq.send('my_test_queue', '{\"order\": {\"id\": 123, \"items\": [{\"sku\": \"A1\", \"qty\": 2}], \"total\": 99.99}}');"
```

## Verify

```bash
docker exec -e PGPASSWORD='<password>' pgmq-postgres-pgmq-admin-ui \
  psql -U postgres -d pgmq -c "SELECT * FROM pgmq.metrics('my_test_queue');"
```
