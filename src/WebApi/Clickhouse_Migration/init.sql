CREATE TABLE IF NOT EXISTS agent_metrics (
    AgentName String,
    Type String,
    Name String,
    Value Float64,
    Unit String,
    TimestampUtc DateTime,
    Metadata String DEFAULT '',
    EventDate Date DEFAULT toDate(TimestampUtc)
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(TimestampUtc)
ORDER BY (AgentName, Type, TimestampUtc);
