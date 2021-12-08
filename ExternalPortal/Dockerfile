ARG VERSION=3.1-alpine3.14

# Get base image and copy project file
FROM mcr.microsoft.com/dotnet/sdk:$VERSION AS publish

WORKDIR /src

# Needs to be enabled to fetch the private repository credentials for NuGet restore.
ARG PAT
ENV PAT ${PAT}

RUN apk update && apk add bash

RUN apk update && apk add -q wget gnupg 

RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS "{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/Ofgem-eServe/_packaging/Ofgem-eServe/nuget/v3/index.json\", \"username\":\"optional\", \"password\":\"${PAT}\"}]}"

COPY ["nuget.config", "."]
COPY ["ExternalPortal/ExternalPortal.csproj", "ExternalPortal/"]

# Restore project
RUN dotnet restore "ExternalPortal/ExternalPortal.csproj" -s "https://pkgs.dev.azure.com/Ofgem-eServe/_packaging/Ofgem-eServe/nuget/v3/index.json" -s "https://api.nuget.org/v3/index.json"

# Copy restored project
COPY . .

# Publish project
RUN dotnet publish "ExternalPortal/ExternalPortal.csproj" -c Release -o /app/publish

# Create final image with entrypoint
FROM mcr.microsoft.com/dotnet/aspnet:$VERSION AS final

WORKDIR /app
COPY --from=publish /app/publish .

RUN apk update && apk add -q icu-libs

# Add user without root privileges
RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app

USER app

ENV AppSettings__EnableHTTPS="false"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://*:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "ExternalPortal.dll"]
