FROM postgres
COPY quartz_schema_postgres.sql /docker-entrypoint-initdb.d/
