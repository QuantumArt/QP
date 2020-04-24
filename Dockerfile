FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
LABEL stage=intermediate

RUN apt-get install -y \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install -y nodejs \
	&& npm install gulp cross-env -g

WORKDIR /src
COPY *.sln NuGet.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY package.json .
COPY package-lock.json .
RUN npm ci

COPY . .
RUN npm run build

WORKDIR /src/siteMvc
RUN dotnet publish "WebMvc.csproj" -c Release -o /app/out -f netcoreapp3.1

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-QP}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/out .
RUN rm -rf /app/hosting.json
EXPOSE 80
ENTRYPOINT ["dotnet", "Quantumart.QP8.WebMvc.dll"]
