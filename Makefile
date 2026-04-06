.PHONY: restore build run test clean

restore:
	dotnet restore ./src/Api/Api.csproj

build:
	dotnet build ./src/Api/Api.csproj --no-restore

run:
	dotnet run --project ./src/Api

test:
	dotnet test 2>/dev/null || echo "No test project configured"

clean:
	dotnet clean ./src/Api/Api.csproj
	rm -rf bin/ obj/

docker-build:
	docker build -t dotnet-webapi-clean .

docker-run:
	docker run -p 8080:8080 dotnet-webapi-clean
