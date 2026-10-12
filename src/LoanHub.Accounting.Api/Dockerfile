FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/LoanHub.Events/LoanHub.Events.csproj LoanHub.Events/
COPY src/LoanHub.Accounting.Domain/LoanHub.Accounting.Domain.csproj LoanHub.Accounting.Domain/
COPY src/LoanHub.Accounting.Application/LoanHub.Accounting.Application.csproj LoanHub.Accounting.Application/
COPY src/LoanHub.Accounting.Infrastructure/LoanHub.Accounting.Infrastructure.csproj LoanHub.Accounting.Infrastructure/
COPY src/LoanHub.Accounting.Api/LoanHub.Accounting.Api.csproj LoanHub.Accounting.Api/
RUN dotnet restore LoanHub.Accounting.Api/LoanHub.Accounting.Api.csproj
COPY src/LoanHub.Events/ LoanHub.Events/
COPY src/LoanHub.Accounting.Domain/ LoanHub.Accounting.Domain/
COPY src/LoanHub.Accounting.Application/ LoanHub.Accounting.Application/
COPY src/LoanHub.Accounting.Infrastructure/ LoanHub.Accounting.Infrastructure/
COPY src/LoanHub.Accounting.Api/ LoanHub.Accounting.Api/
RUN dotnet publish LoanHub.Accounting.Api/LoanHub.Accounting.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8082
EXPOSE 8082
ENTRYPOINT ["dotnet", "LoanHub.Accounting.Api.dll"]
