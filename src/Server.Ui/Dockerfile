FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS production

# Copy built files
COPY --from=build /app/dist /usr/share/nginx/html

RUN echo '#!/bin/sh' > /docker-entrypoint.d/40-inject-env.sh && \
    echo 'cat > /usr/share/nginx/html/env-config.js << EOF' >> /docker-entrypoint.d/40-inject-env.sh && \
    echo 'window.__ENV__ = {' >> /docker-entrypoint.d/40-inject-env.sh && \
    echo '  API_URL: "${API_URL:-http://localhost:5140/api/v1}"' >> /docker-entrypoint.d/40-inject-env.sh && \
    echo '};' >> /docker-entrypoint.d/40-inject-env.sh && \
    echo 'EOF' >> /docker-entrypoint.d/40-inject-env.sh && \
    chmod +x /docker-entrypoint.d/40-inject-env.sh

# Nginx config for SPA routing
RUN echo 'server { \
    listen 80; \
    listen [::]:80; \
    server_name localhost; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
    location /assets { \
        expires 1y; \
        add_header Cache-Control "public, immutable"; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
