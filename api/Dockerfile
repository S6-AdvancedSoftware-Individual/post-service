# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PostService.Api/PostService.Api.csproj", "PostService.Api/"]
COPY ["PostService.Application/PostService.Application.csproj", "PostService.Application/"]
COPY ["PostService.Infrastructure/PostService.Infrastructure.csproj", "PostService.Infrastructure/"]
COPY ["PostService.Domain/PostService.Domain.csproj", "PostService.Domain/"]
RUN dotnet restore "PostService.Api/PostService.Api.csproj"
COPY . .
WORKDIR "/src/PostService.Api"
RUN dotnet build "PostService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostService.Api.dll"]