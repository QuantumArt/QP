FROM node:16-bullseye as front-builder
LABEL stage=intermediate-front
WORKDIR /src
RUN npm install gulp cross-env -g
COPY package.json package-lock.json .babelrc .browserslistrc ./
RUN npm ci

COPY siteMvc ./siteMvc
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL stage=intermediate-back

WORKDIR /src
COPY *.sln NuGet.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY . .
COPY --from=front-builder /src/siteMvc/Scripts/build ./siteMvc/Scripts/build
COPY --from=front-builder /src/siteMvc/Static/build ./siteMvc/Static/build

WORKDIR /src/siteMvc
RUN dotnet publish "WebMvc.csproj" -c Release -o /app/out -f net6.0

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-QP}

ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
COPY --from=build-env /app/out .
RUN rm -rf /app/hosting.json
EXPOSE 80
ENTRYPOINT ["dotnet", "Quantumart.QP8.WebMvc.dll"]
