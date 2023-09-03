# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Dockube.Api/Dockube.Api.csproj", "Dockube.Api/"]
COPY ["Dockube.Shared/Dockube.Shared.csproj", "Dockube.Shared/"]
RUN dotnet restore "Dockube.Api/Dockube.Api.csproj"
COPY . .
WORKDIR "/src/Dockube.Api"
RUN dotnet build "Dockube.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dockube.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install Git
RUN apt-get update && apt-get install -y git

# Set environment variables
ENV GIT_USERNAME="dockube"
ENV GIT_EMAIL="jan.banan@gmail.com"
ENV GIT_ACCESS_TOKEN="aa6f5c99f071abc0eb38c0c72a2c9e283b915ab7"

# Configure Git
RUN git config --global user.name "$GIT_USERNAME"
RUN git config --global user.email "$GIT_EMAIL"
RUN git config --global http.extraHeader "Authorization: Bearer $(echo -n $GIT_ACCESS_TOKEN | base64)"

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dockube.Api.dll"]
