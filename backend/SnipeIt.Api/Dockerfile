FROM ubuntu:24.04 AS build

ARG DOTNET_VERSION=8.0
ENV DEBIAN_FRONTEND=noninteractive \
	DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /src

RUN apt-get update -qqy \
	&& apt-get install -qqy --no-install-recommends \
		ca-certificates \
		curl \
		gnupg \
		wget \
	&& mkdir -p /etc/apt/keyrings \
	&& wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg \
	&& chmod go+r /etc/apt/keyrings/microsoft.gpg \
	&& echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/ubuntu/24.04/prod noble main" > /etc/apt/sources.list.d/microsoft-prod.list \
	&& apt-get update -qqy \
	&& apt-get install -qqy --no-install-recommends dotnet-sdk-${DOTNET_VERSION} \
	&& rm -rf /var/lib/apt/lists/*

COPY SnipeIt.Api.csproj ./
RUN dotnet restore ./SnipeIt.Api.csproj

COPY . .
RUN dotnet publish ./SnipeIt.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM ubuntu:24.04 AS final

ARG DOTNET_VERSION=8.0
ENV DEBIAN_FRONTEND=noninteractive \
	ASPNETCORE_URLS=http://+:8080 \
	DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /app
EXPOSE 8080

RUN apt-get update -qqy \
	&& apt-get install -qqy --no-install-recommends \
		ca-certificates \
		curl \
		gnupg \
		wget \
	&& mkdir -p /etc/apt/keyrings \
	&& wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg \
	&& chmod go+r /etc/apt/keyrings/microsoft.gpg \
	&& echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/ubuntu/24.04/prod noble main" > /etc/apt/sources.list.d/microsoft-prod.list \
	&& apt-get update -qqy \
	&& apt-get install -qqy --no-install-recommends aspnetcore-runtime-${DOTNET_VERSION} \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SnipeIt.Api.dll"]
