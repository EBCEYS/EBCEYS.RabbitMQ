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

#### Пример конфигурации:
```cs
configBuilder = new();
configBuilder.AddConnectionFactory(new()
{
    HostName = "Kuznetsovy-Server",
    UserName = "ebcey1",
    Password = "123"
});
configBuilder.AddQueueConfiguration(new("ExampleQueue", autoDelete: true));
RabbitMQConfiguration config = configBuilder.Build();
IHost host = Host.CreateDefaultBuilder(args)
    .UseNLog()
    .ConfigureLogging(log =>
    {
        log.AddNLog("nlog.config");
    })
    .ConfigureServices(services =>
    {
        services.AddRabbitMQController<ExampleController>();

        services.AddRabbitMQMappedServer(config);
    })
    .Build();
host.Run();
```

### EBCEYS.RabbitMQ.Client
Реализация publisher-a для работы с брокером сообщений RabbitMQ.

Через клиент, в зависимости от конфигурации, возможна отправка сообщений и "RPC" запросов.

Для отправки сообщений используется метод SendMessageAsync.

Для отправки "RPC" запросов используется метод SendRequestAsync.


## Изменения
### v1.2.0
1) Так как использование Microsoft.Text.Json вызывало ошибки в работе контроллера - было принято решение перейти на Newtownsoft.Json.
2) В случае если возникли ошибки во время обработки сообщения/запроса, сообщение будет приниматься.
### v1.1.6
1) Исправлена ошибка, когда, при атрибутах метода string, некорректно парсились параметры запроса в контроллере.
### v1.1.5
1) Добавил простые конструкторы для конфигурации.
### v1.1.4
1) В ServiceCollectionExtensions добавлен метод для добавления RabbitMQClient-a.
### v1.1.3
1) Переделаны контроллеры. Теперь они scoped.
2) Убран метод FixRabbitMQControllers для IServiceCollection
### v1.1.2
1) Исправлена ошибка, когда при использовании метода AddRabbitMQController, MappedServer мог не видеть контроллеры.
### v1.1.1 Большая переработка проекта
Фичи:
1) Добавил проекты с примерами работы клиента и сервера;
2) Добавил базовый интерфейс для контроллера;
3) Добавил методы расширения IServiceCollection, через который идет настройка сервера и контроллеров;
4) RabbitMQMappedServer теперь не требует сервиса самого сервера, а инициирует его сам.
5) Убрал кучу лишних конструкторов;
6) Добавил дебаг логирование в сервисы;

Фиксы:

1) Исправлена ошибка, когда таймаут ожидания ответа на запрос учитывался в миллисекундах.
2) Исправлена ошибка, когда не стартовал ресивер, если в настройках не указать, что он асинхронный;
3) Исправил ошибку, когда при отправке ответа на запрос, MappedServer выдавал исключение.
### v1.0.4
1) Изменил алгоритм работы при остановке сервиса. Теперь только закрывает соединение.
2) Добавил реализацию клиента (EBCEYS.RabbitMQ.Client). Через него возможна отправка сообщений и запросов на контроллеры.
### v1.0.3
1) Переименовал атрибут для методов RabbitMQ (RabbitMQMethodName -> RabbitMQMethod).
2) Исправил недоработку когда для парсинга сообщения в ресивере не использовались SerializerOptions.
3) Добавил тесты для тестирования в своей среде (во время CI/CD запускаться не будут).


## Планы на развитие
1) Дебаг в зависимости от найденных ошибок / отзывов.