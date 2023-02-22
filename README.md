# EBCEYS.RabbitMQ

## Описание

Библиотека для работы с брокером сообщений RabbitMQ.

### RabbitMQ.Server.Service

Реализация consumer-a как IHostedService.

### RabbitMQ.Server.MapedService

Попытка реализации сервиса consumer-a через контроллеры (а-ля ControllerBase для http).

### Запрос для обработки
```json
public class RabbitMQRequestData
    {
        public object[]? Params { get; set; }
        public string? Method { get; set; }
    }
```