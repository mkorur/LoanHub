FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/LoanHub.Events/LoanHub.Events.csproj LoanHub.Events/
COPY src/LoanHub.Loan.Domain/LoanHub.Loan.Domain.csproj LoanHub.Loan.Domain/
COPY src/LoanHub.Loan.Application/LoanHub.Loan.Application.csproj LoanHub.Loan.Application/
COPY src/LoanHub.Loan.Infrastructure/LoanHub.Loan.Infrastructure.csproj LoanHub.Loan.Infrastructure/
COPY src/LoanHub.Loan.Api/LoanHub.Loan.Api.csproj LoanHub.Loan.Api/
RUN dotnet restore LoanHub.Loan.Api/LoanHub.Loan.Api.csproj
COPY src/LoanHub.Events/ LoanHub.Events/
COPY src/LoanHub.Loan.Domain/ LoanHub.Loan.Domain/
COPY src/LoanHub.Loan.Application/ LoanHub.Loan.Application/
COPY src/LoanHub.Loan.Infrastructure/ LoanHub.Loan.Infrastructure/
COPY src/LoanHub.Loan.Api/ LoanHub.Loan.Api/
RUN dotnet publish LoanHub.Loan.Api/LoanHub.Loan.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081
ENTRYPOINT ["dotnet", "LoanHub.Loan.Api.dll"]
