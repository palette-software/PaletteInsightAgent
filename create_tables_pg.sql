CREATE TABLE countersamples
(
   timestamp TIMESTAMP,
   machine VARCHAR,
   category VARCHAR,
   instance VARCHAR,
   process VARCHAR,
   name VARCHAR,
   value DOUBLE PRECISION
);

CREATE TABLE filter_state_audit
(
   ts TIMESTAMP,
   pid BIGINT,
   tid INT,
   req VARCHAR,
   sess VARCHAR,
   site VARCHAR,
   username VARCHAR,
   filter_name VARCHAR,
   filter_vals VARCHAR,
   workbook VARCHAR,
   view VARCHAR,
   hostname VARCHAR,
   user_ip VARCHAR
);

CREATE TABLE serverlogs
(
   filename VARCHAR,
   host_name VARCHAR,
   ts TIMESTAMP,
   pid BIGINT,
   tid INT,
   sev VARCHAR,
   req VARCHAR,
   sess VARCHAR,
   site VARCHAR,
   username VARCHAR,
   k VARCHAR, --json key
   v VARCHAR -- json value
);

CREATE TABLE threadinfo
(
   host_name VARCHAR,
   process VARCHAR,
   ts TIMESTAMP,
   pid BIGINT,
   tid BIGINT,
   cpu_time BIGINT,
   poll_cycle_ts TIMESTAMP,
   start_ts TIMESTAMP,
   thread_count INT,
   working_set BIGINT
);
