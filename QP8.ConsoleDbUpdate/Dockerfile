FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL stage=intermediate
ENV TZ=Russia/Moscow

WORKDIR /src
COPY ./*.sln ./NuGet.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY . .

WORKDIR /src/QP8.ConsoleDbUpdate
RUN dotnet publish "ConsoleDbUpdate.csproj" -c Release -o /app/out -f net6.0

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ConsoleDbUpdate.dll"]
