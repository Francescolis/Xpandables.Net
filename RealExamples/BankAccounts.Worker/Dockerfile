# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["nuget.config", "."]
COPY ["RealExamples/BankAccounts.Worker/BankAccounts.Worker.csproj", "RealExamples/BankAccounts.Worker/"]
COPY ["AspNetCore.Net/AspNetCore.Net.csproj", "AspNetCore.Net/"]
COPY ["RealExamples/BankAccounts.Domain/BankAccounts.Domain/BankAccounts.Domain.csproj", "RealExamples/BankAccounts.Domain/BankAccounts.Domain/"]
COPY ["System.Composition/System.Composition.csproj", "System.Composition/"]
COPY ["System.Primitives/System.Primitives.csproj", "System.Primitives/"]
COPY ["System.Events.Data/System.Events.Data.csproj", "System.Events.Data/"]
COPY ["System.Data/System.Data.csproj", "System.Data/"]
COPY ["System.AsyncPaged/System.AsyncPaged.csproj", "System.AsyncPaged/"]
COPY ["System.Events/System.Events.csproj", "System.Events/"]
COPY ["System.Results.Tasks/System.Results.Tasks.csproj", "System.Results.Tasks/"]
COPY ["System.Results.Pipelines/System.Results.Pipelines.csproj", "System.Results.Pipelines/"]
COPY ["System.Entities/System.Entities.csproj", "System.Entities/"]
COPY ["System.Results/System.Results.csproj", "System.Results/"]
COPY ["System.Validation/System.Validation.csproj", "System.Validation/"]
COPY ["RealExamples/BankAccounts.Infrastructure/BankAccounts.Infrastructure.csproj", "RealExamples/BankAccounts.Infrastructure/"]
COPY ["System.Entities.EntityFramework/System.Entities.EntityFramework.csproj", "System.Entities.EntityFramework/"]
COPY ["RealExamples/BankAccounts.ServiceDefaults/BankAccounts.ServiceDefaults.csproj", "RealExamples/BankAccounts.ServiceDefaults/"]
RUN dotnet restore "./RealExamples/BankAccounts.Worker/BankAccounts.Worker.csproj"
COPY . .
WORKDIR "/src/RealExamples/BankAccounts.Worker"
RUN dotnet build "./BankAccounts.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BankAccounts.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankAccounts.Worker.dll"]