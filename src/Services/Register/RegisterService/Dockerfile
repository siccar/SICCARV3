# To run this Dockerfile you need:
# 1. An Azure DevOps PAT as a build arg FEED_ACCESSTOKEN (private Azure Feed requires a PAT)
# 2. Place the PAT value in a .env file and configure docker-compose-override.yml to use the build arg FEED_ACCESSTOKEN
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /app
ARG BUILDARGS=Release
ENV BUILDCONFIG=$BUILDARGS
# Install the Azure Artifact Credential Provider (will use a PAT)
#RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | sh
RUN curl -fsSL https://aka.ms/install-artifacts-credprovider.sh  | sh
COPY ["src/Services/Register/RegisterService/RegisterService.csproj", "src/Services/Register/RegisterService/"]
COPY ["src/Services/Register/RegisterService/nuget.config", "src/Services/Register/RegisterService/"]
COPY ["src/Services/Register/RegisterCoreMongoDBStorage/RegisterCoreMongoDBStorage.csproj", "src/Services/Register/RegisterCoreMongoDBStorage/"]
COPY ["src/Services/Register/RegisterCoreMongoDBStorage/nuget.config", "src/Services/Register/RegisterCoreMongoDBStorage/"]
COPY ["src/Services/Register/RegisterCore/RegisterCore.csproj", "src/Services/Register/RegisterCore/"]
COPY ["src/Services/Register/RegisterCore/nuget.config", "src/Services/Register/RegisterCore/"]
COPY ["src/Common/SiccarCommon/SiccarCommon.csproj", "src/Common/SiccarCommon/"]
COPY ["src/Common/SiccarCommon/nuget.config", "src/Common/SiccarCommon/"]
COPY ["src/Common/SiccarCommonServiceClients/SiccarCommonServiceClients.csproj", "src/Common/SiccarCommonServiceClients/"]
COPY ["src/Common/SiccarCommonServiceClients/nuget.config", "src/Common/SiccarCommonServiceClients/"]
ARG FEED_ACCESSTOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://projectbob.pkgs.visualstudio.com/SICCARV3/_packaging/siccarv3feed/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"
RUN echo $VSS_NUGET_EXTERNAL_FEED_ENDPOINTS
RUN dotnet restore "src/Services/Register/RegisterService/RegisterService.csproj"
COPY . .
RUN dotnet add "src/Services/Register/RegisterService/RegisterService.csproj" package Microsoft.ApplicationInsights.Kubernetes.HostingStartup
RUN dotnet build  "src/Services/Register/RegisterService/RegisterService.csproj" -c ${BUILDCONFIG} -o /app/build

FROM build AS publish
RUN dotnet publish  "src/Services/Register/RegisterService/RegisterService.csproj" -c ${BUILDCONFIG} -o /app/publish

# Testrunner image
FROM build AS testrunner
WORKDIR /app/src/Services/Register/tests
COPY src/Services/Register/RegisterTests/*.csproj .
COPY src/Services/Register/RegisterTests/nuget.config .
RUN dotnet restore RegisterTests.csproj
COPY src/Services/Register/RegisterTests/. .
ENTRYPOINT ["dotnet", "test", "--logger:trx"]

FROM base AS final
# Create a group and user so we are not running our container and application as root and thus user 0 which is a security issue.
RUN addgroup --system --gid 2600 siccargroup \
    && adduser --system --uid 1000 --ingroup siccargroup --shell /bin/sh siccaruser
WORKDIR /app
RUN mkdir -p /local/
COPY --from=publish /app/publish .
RUN chown siccaruser:siccargroup /local
RUN chown siccaruser:siccargroup /app
# copy the localhost test cert
COPY ["components/selfsigned.crt_","/usr/local/share/ca-certificates/localhost.crt"]
COPY ["components/selfsigned.crt_","/etc/ssl/certs/localhost.pem"]
RUN update-ca-certificates --fresh
USER 1000
ENTRYPOINT ["dotnet", "RegisterService.dll"]