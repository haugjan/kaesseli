@echo off
docker compose -f "%~dp0Kaesseli.Deploy\docker-compose.yml" up -d aspire-dashboard
start http://localhost:18888
