FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
LABEL stage=intermediate

RUN apt-get install -y \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install -y nodejs \
	&& apt-get install unzip

WORKDIR /src
COPY NuGet.config .
COPY projectstructure.zip .
RUN unzip projectstructure.zip && dotnet restore

RUN npm install gulp cross-env -g
COPY package.json .
COPY package-lock.json .
RUN npm ci

COPY . .
RUN npm run build

WORKDIR /src/siteMvc
RUN dotnet publish "WebMvc.csproj" -c Release -o /app/out -f netcoreapp2.2

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
ARG SERVICE_VERSION=0.0.0
ENV ServiceVersion=${SERVICE_VERSION}
WORKDIR /app
COPY --from=build-env /app/out .
RUN rm -rf /app/hosting.json
EXPOSE 80
ENTRYPOINT ["dotnet", "Quantumart.QP8.WebMvc.dll"]
