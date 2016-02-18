CREATE TABLE pstableau.countersamples
(
   timestamp TIMESTAMP,
   machine VARCHAR,
   category VARCHAR,
   instance VARCHAR,
   process VARCHAR,
   name VARCHAR,
   value DOUBLE PRECISION
)
WITH (appendonly=true, orientation=column, compresstype=quicklz)
DISTRIBUTED BY (timestamp);

CREATE TABLE pstableau.filter_state_audit
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
)
WITH (appendonly=true, orientation=column, compresstype=quicklz)
DISTRIBUTED BY (ts);

CREATE TABLE pstableau.serverlogs
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
   v VARCHAR -- jason value
)
WITH (appendonly=true, orientation=column, compresstype=quicklz)
DISTRIBUTED BY (ts);

CREATE TABLE pstableau.threadinfo
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
)
WITH (appendonly=true, orientation=column, compresstype=quicklz)
DISTRIBUTED BY (ts);
