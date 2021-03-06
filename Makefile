.PHONY: init_broker shutdown_broker

init_broker:	
	@echo command | date
	@echo Initializing Kafka broker
	docker-compose -f docker-compose.yml up -d
	docker exec kafka  bash -c "kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 1 --topic test-avro;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-gzip;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 1 --topic test-json;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-json-gzip;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-protobuf;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-protobuf-gzip;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-protobuf-gzip-2;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-pause-resume;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-protobuf-sr;kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 2 --topic test-json-sr"

shutdown_broker:
	@echo command | date
	@echo Shutting down kafka broker
	docker-compose -f docker-compose.yml down

restore:
	dotnet restore src/KafkaFlow.sln

build:
	dotnet build src/KafkaFlow.sln

unit_tests:
	@echo command | date
	@echo Running unit tests
	dotnet test src/KafkaFlow.UnitTests/KafkaFlow.UnitTests.csproj --framework netcoreapp2.1 --logger "console;verbosity=detailed"
	dotnet test src/KafkaFlow.UnitTests/KafkaFlow.UnitTests.csproj --framework netcoreapp3.1 --logger "console;verbosity=detailed"

integration_tests:
	@echo command | date
	make init_broker
	@echo Running integration tests
	dotnet test src/KafkaFlow.IntegrationTests/KafkaFlow.IntegrationTests.csproj -c Release --framework netcoreapp3.1 --logger "console;verbosity=detailed"
	make shutdown_broker
