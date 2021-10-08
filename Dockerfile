FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
LABEL stage=intermediate

RUN apt-get install -y \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install -y nodejs

RUN	npm install gulp cross-env -g
COPY package.json package-lock.json ./
RUN npm ci

WORKDIR /src
COPY *.sln NuGet.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY . .
RUN npm run build

ARG PUBLISH_ARG
RUN echo "variable ${PUBLISH_ARG}"

WORKDIR /src/siteMvc
RUN dotnet publish "WebMvc.csproj" -c Release -o /app/out -f netcoreapp3.1 ${PUBLISH_ARG}

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
