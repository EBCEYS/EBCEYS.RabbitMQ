# EBCEYS.RabbitMQ

[![.NET](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml/badge.svg)](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml)

## Описание

Библиотека для работы с брокером сообщений RabbitMQ.

### EBCEYS.RabbitMQ.Server.MappedService.RabbitMQServer

Реализация consumer-a как IHostedService.

Стоит отметить что в данном случае consumer всегда будет асинхронный.

### EBCEYS.RabbitMQ.Server.MappedService.RabbitMQMappedServer

Попытка реализации сервиса consumer-a через контроллеры (а-ля ControllerBase для http).

Методы контроллера должны быть асинхронными.

### EBCEYS.RabbitMQ.Client
Реализация publisher-a для работы с брокером сообщений RabbitMQ.

Через клиент, в зависимости от конфигурации, возможна отправка сообщений и "RPC" запросов.

Для отправки сообщений используется метод SendMessageAsync.

Для отправки "RPC" запросов используется метод SendRequestAsync.


## Изменения
### v1.0.4
1) Изменил алгоритм работы при остановке сервиса. Теперь только закрывает соединение.
2) Добавил реализацию клиента (EBCEYS.RabbitMQ.Client). Через него возможна отправка сообщений и запросов на контроллеры.
### v1.0.3
1) Переименовал атрибут для методов RabbitMQ (RabbitMQMethodName -> RabbitMQMethod).
2) Исправил недоработку когда для парсинга сообщения в ресивере не использовались SerializerOptions.
3) Добавил тесты для тестирования в своей среде (во время CI/CD запускаться не будут).


## Планы на развитие
1) Дебаг в зависимости от найденных ошибок / отзывов.
2) Добавить проект примера работы, в котором будет показана работа с контроллерами и клиентом.