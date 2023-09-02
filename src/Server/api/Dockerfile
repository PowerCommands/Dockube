#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Dockube.Api/Dockube.Api.csproj", "Dockube.Api/"]
RUN dotnet restore "Dockube.Api/Dockube.Api.csproj"
COPY . .
WORKDIR "/src/Dockube.Api"
RUN apt-get update && apt-get install -y git

ENV GIT_USERNAME="Your Name"
ENV GIT_EMAIL="you@example.com"
ENV GIT_ACCESS_TOKEN="your_actual_access_token"

RUN git config --global user.name "$GIT_USERNAME"
RUN git config --global user.email "$GIT_EMAIL"
RUN git config --global http.extraHeader "Authorization: Bearer $(echo -n $GIT_ACCESS_TOKEN | base64)"
RUN dotnet build "Dockube.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dockube.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dockube.Api.dll"]