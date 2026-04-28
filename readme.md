# RateLimiter

Проект состоит из нескольких .NET 9 сервисов вокруг пользовательского API и rate limiter. Основная идея: лимиты запросов хранятся отдельно, reader-сервис держит актуальную копию лимитов в памяти, события пользовательских запросов приходят через Kafka, а факт блокировки пользователя на конкретном endpoint сохраняется в Redis.

## Состав проекта

### `UserService`

gRPC-сервис управления пользователями. Работает с PostgreSQL через Dapper и SQL-функции из папки `UserService/Database`.

Что умеет:

- `CreateUser` - создает пользователя.
- `GetUserById` - возвращает пользователя по id.
- `GetUserByName` - ищет пользователей по имени и фамилии.
- `UpdateUser` - обновляет данные пользователя.
- `DeleteUser` - удаляет пользователя.

Как работает:

- слушает порт `5002`;
- требует metadata header `user_id` для каждого gRPC-запроса;
- через interceptor `Auth` проверяет наличие `user_id`;
- через interceptor `RateLimit` проверяет Redis-ключ `blocked:{user_id}:{endpoint}`;
- если пользователь заблокирован для endpoint, возвращает gRPC status `ResourceExhausted`;
- хранит пользователей в PostgreSQL `usersdb`.

### `RateLimiter.Writer`

gRPC-сервис для управления правилами rate limit. Это write-side часть системы: сюда добавляются, обновляются и удаляются лимиты.

Что умеет:

- `CreateLimit` - создает лимит для route.
- `GetLimitByRoute` - получает лимит по route.
- `UpdateLimit` - обновляет лимит.
- `DeleteLimit` - удаляет лимит.

Как работает:

- слушает порт `5001`;
- хранит лимиты в MongoDB, база `writerdb`, коллекция `rate_limits`;
- проверяет входные данные через FluentValidation;
- создает индекс в MongoDB при старте;
- изменения в MongoDB потом видит `RateLimiter.Reader` через change streams.

### `RateLimiter.Reader`

gRPC-сервис чтения лимитов и обработки потока событий запросов. Это read-side часть rate limiter.

Что умеет:

- `GetAllLimits` - возвращает все лимиты из in-memory состояния.
- читает события пользовательских запросов из Kafka topic `user-events`.
- считает количество запросов пользователя к endpoint за последнюю минуту.
- при превышении лимита кладет блокировку в Redis на 5 минут.

Как работает:

- слушает порт `5000`;
- при старте загружает лимиты из MongoDB в память;
- подписывается на MongoDB change streams и обновляет in-memory лимиты после insert/update/delete;
- читает Kafka-сообщения вида `{"UserId":123,"Endpoint":"GetUserById"}`;
- нормализует endpoint к нижнему регистру;
- Redis-ключ блокировки имеет формат `blocked:{userId}:{endpoint}`.

Важно: для MongoDB change streams нужен replica set. В compose Mongo запускается с `--replSet rs0`, но после первого старта replica set может потребовать инициализацию.

### `UserRequestsKafkaGenerator`

Консольный генератор Kafka-событий. Нужен для ручной проверки rate limiter без настоящего клиентского трафика.

Как работает:

- подключается к Kafka `localhost:9092`;
- отправляет события в topic `user-events`;
- при старте создает два demo-потока запросов;
- поддерживает интерактивные команды:

```text
add <userId> <endpoint> <rpm>
list
update <id> [rpm=<int>] [endpoint=<str>]
remove <id>
help
quit
```

## Инфраструктура

В проекте используются:

- PostgreSQL - данные `UserService`.
- MongoDB - лимиты rate limiter.
- Redis - временные блокировки пользователей.
- Kafka + Zookeeper - поток событий пользовательских запросов.
- Kafka UI - веб-интерфейс для Kafka на `http://localhost:8080`.
- Mongo Express - веб-интерфейс для MongoDB на `http://localhost:8081`.
- pgAdmin - веб-интерфейс для PostgreSQL на `http://localhost:5050`.

## Запуск

Нужны Docker, Docker Compose и .NET SDK с поддержкой `net9.0`.

### 1. Запустить инфраструктуру

Из корня репозитория:

```bash
docker compose up -d
```

Этот compose запускает MongoDB, Redis, Kafka, Kafka UI и Mongo Express.

PostgreSQL для `UserService` запускается отдельным compose-файлом:

```bash
docker compose -f UserService/docker-compose.yml up -d
```

При первом запуске MongoDB инициализируйте replica set:

```bash
docker exec -it writer-mongo mongosh --eval "rs.initiate()"
```

Если replica set уже был создан раньше, команда может вернуть сообщение, что он уже инициализирован.

### 2. Запустить сервисы

Откройте отдельные терминалы и выполните:

```bash
dotnet run --project RateLimiter.Writer/RateLimiter.Writer.csproj
```

```bash
dotnet run --project RateLimiter.Reader/RateLimiter.Reader.csproj
```

```bash
dotnet run --project UserService/UserService.csproj
```

Опционально запустите генератор событий:

```bash
dotnet run --project UserRequestsKafkaGenerator/UserRequestsKafkaGenerator.csproj
```

## Порты

| Компонент | Порт |
| --- | --- |
| `RateLimiter.Reader` | `5000` |
| `RateLimiter.Writer` | `5001` |
| `UserService` | `5002` |
| PostgreSQL | `5432` |
| Redis | `6379` |
| Kafka | `9092` |
| Kafka UI | `8080` |
| MongoDB | `27017` |
| Mongo Express | `8081` |
| pgAdmin | `5050` |

## Проверка и тестирование

Автоматических test-проектов в репозитории сейчас нет, поэтому базовая проверка состоит из сборки и ручных интеграционных сценариев.

### Сборка

```bash
dotnet build RateLimiter.sln
```

### Запуск тестов

Команда безопасна для будущих test-проектов, но сейчас она не найдет отдельные тесты:

```bash
dotnet test RateLimiter.sln
```

### Ручная интеграционная проверка

1. Запустите инфраструктуру и сервисы.
2. Через gRPC-клиент создайте лимит в `RateLimiter.Writer`, например для route `getuserbyid`.
3. Запустите `RateLimiter.Reader`, чтобы он загрузил лимит из MongoDB.
4. Запустите `UserRequestsKafkaGenerator`.
5. Добавьте поток событий выше лимита:

```text
add 123 GetUserById 120
```

6. Когда reader увидит превышение, он создаст Redis-ключ `blocked:123:getuserbyid`.
7. Вызовите `UserService/GetUserById` с metadata `user_id: 123`.
8. Пока Redis-ключ жив, сервис должен вернуть `ResourceExhausted`.

Для вызовов gRPC удобно использовать Postman, BloomRPC, grpcurl или любой другой gRPC-клиент. Важно передавать metadata `user_id`, иначе `UserService` вернет `Unauthenticated`.

## Конфигурация по умолчанию

Основные параметры лежат в `appsettings.json` каждого сервиса:

- `UserService/appsettings.json` - строки подключения PostgreSQL и Redis.
- `RateLimiter.Writer/appsettings.json` - MongoDB.
- `RateLimiter.Reader/appsettings.json` - MongoDB, Redis, Kafka.
- `UserRequestsKafkaGenerator/appsettings.json` - Kafka bootstrap servers и topic.

Если сервисы запускаются не с хоста, а внутри Docker-сети, `localhost` в конфигурации нужно заменить на имена compose-сервисов: `mongo`, `redis`, `kafka`, `postgres`.
