# EBCEYS.RabbitMQ

[![.NET](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml/badge.svg)](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml)

## Описание

Библиотека для работы с брокером сообщений RabbitMQ.

### EBCEYS.RabbitMQ.Server.Service

Реализация consumer-a как IHostedService.

### EBCEYS.RabbitMQ.Server.MappedService

Попытка реализации сервиса consumer-a через контроллеры (а-ля ControllerBase для http).

### Запрос для обработки
```json
public class RabbitMQRequestData
    {
        public object[]? Params { get; set; }
        public string? Method { get; set; }
    }
```