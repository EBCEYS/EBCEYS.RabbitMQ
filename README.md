# EBCEYS.RabbitMQ

[![.NET](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml/badge.svg)](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml)

## Описание

Библиотека для работы с брокером сообщений RabbitMQ.

### EBCEYS.RabbitMQ.Server.MappedService.RabbitMQServer

Реализация consumer-a как IHostedService.

### EBCEYS.RabbitMQ.Server.MappedService.RabbitMQMappedServer

Попытка реализации сервиса consumer-a через контроллеры (а-ля ControllerBase для http).

### Запрос для обработки
```json
public class RabbitMQRequestData
    {
        public object[]? Params { get; set; }
        public string? Method { get; set; }
    }
```


## Изменения
### v1.0.3
1) Переименовал атрибут для методов RabbitMQ (RabbitMQMethodName -> RabbitMQMethod).
2) Исправил недоработку когда для парсинга сообщения в ресивере не использовались SerializerOptions.
3) Добавил тесты для тестирования в своей среде (во время CI/CD запускаться не будут).