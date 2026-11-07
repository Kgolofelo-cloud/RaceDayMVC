# Stage 1: Base image for running the app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080
 
# Stage 2: Build the application

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
 
# Copy only the MVC project file and restore dependencies

COPY ["RaceDayMVC.csproj", "./"]

RUN dotnet restore "./RaceDayMVC.csproj"
 
# Copy the rest of the MVC code and build

COPY . .

RUN dotnet build "RaceDayMVC.csproj" -c Release -o /app/build
 
# Stage 3: Publish the application

FROM build AS publish

RUN dotnet publish "RaceDayMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false
 
# Stage 4: Final production image

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]
 