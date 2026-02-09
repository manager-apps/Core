import { Box } from "@mui/material";
import { getGrafanaBaseUrl } from "../../api/axios";

export const MetricsPage: React.FC = () => {
    const grafanaUrl = `${getGrafanaBaseUrl()}/d/clickhouse-metrics/clickhouse-metrics?orgId=1&from=now-1h&to=now&timezone=browser&refresh=5s&kiosk`;

    return (
        <Box sx={{ height: "calc(100vh - 64px)", width: "100%" }}>
            <iframe
                src={grafanaUrl}
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
