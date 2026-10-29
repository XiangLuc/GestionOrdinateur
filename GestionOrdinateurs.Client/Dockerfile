# =========================
# Build (Blazor WebAssembly)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj ./GestionOrdinateurs.Client/
RUN dotnet restore ./GestionOrdinateurs.Client

# Copy the remaining source code
COPY GestionOrdinateurs.Client/ ./GestionOrdinateurs.Client/
RUN dotnet publish ./GestionOrdinateurs.Client -c Release -o out

# =========================
# Runtime : les fichiers statiques sont servis par nginx
# =========================
FROM nginx:alpine AS runtime
COPY --from=build /app/out/wwwroot /usr/share/nginx/html
COPY GestionOrdinateurs.Client/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 5000 (cf. nginx.conf)
EXPOSE 5000
