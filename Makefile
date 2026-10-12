.PHONY: infra-up infra-down up-all run-loan run-accounting build

infra-up:
	docker compose up -d

infra-down:
	docker compose down

up-all:
	docker compose --profile app up -d --build

build:
	dotnet build LoanHub.sln

run-loan:
	dotnet run --project src/LoanHub.Loan.Api/LoanHub.Loan.Api.csproj

run-accounting:
	dotnet run --project src/LoanHub.Accounting.Api/LoanHub.Accounting.Api.csproj
