import { Box } from "@mui/material";

const GRAFANA_URL =
    "http://localhost:3000/d/clickhouse-metrics/clickhouse-metrics?orgId=1&from=now-1h&to=now&timezone=browser&refresh=5s&kiosk";

export const MetricsPage: React.FC = () => {
    return (
        <Box sx={{ height: "calc(100vh - 64px)", width: "100%" }}>
            <iframe
                src={GRAFANA_URL}
                style={{
                    width: "100%",
                    height: "100%",
                    border: "none",
                }}
                title="Grafana Metrics Dashboard"
                allow="fullscreen"
            />
        </Box>
    );
};
