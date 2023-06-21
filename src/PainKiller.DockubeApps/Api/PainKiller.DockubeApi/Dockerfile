#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

# Install required dependencies
RUN apt-get update \
    && apt-get install -y apt-transport-https gnupg2 curl

# Download and install the kubectl binary
RUN curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl" \
    && install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Api/PainKiller.DockubeApi/PainKiller.DockubeApi.csproj", "Api/PainKiller.DockubeApi/"]
RUN dotnet restore "Api/PainKiller.DockubeApi/PainKiller.DockubeApi.csproj"
COPY . .
WORKDIR "/src/Api/PainKiller.DockubeApi"
RUN dotnet build "PainKiller.DockubeApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PainKiller.DockubeApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PainKiller.DockubeApi.dll"]