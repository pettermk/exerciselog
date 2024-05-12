FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o build

FROM cgr.dev/chainguard/nginx:latest
COPY --from=builder /app/build/wwwroot/ /usr/share/nginx/html/
