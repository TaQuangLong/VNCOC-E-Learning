# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ChurchLearn.Api/ChurchLearn.Api.csproj", "src/ChurchLearn.Api/"]
RUN dotnet restore "src/ChurchLearn.Api/ChurchLearn.Api.csproj"

COPY . .
WORKDIR "/src/src/ChurchLearn.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChurchLearn.Api.dll"]
