# EBCEYS.RabbitMQ

[![.NET](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml/badge.svg)](https://github.com/EBCEYS/EBCEYS.RabbitMQ/actions/workflows/dotnet.yml)

## Описание

Библиотека для работы с брокером сообщений RabbitMQ.

### EBCEYS.RabbitMQ.Client

Реализация publisher-a для работы с брокером сообщений RabbitMQ.

Через клиент, в зависимости от конфигурации, возможна отправка сообщений и "RPC" запросов.

Для отправки сообщений используется метод SendMessageAsync.

Для отправки "RPC" запросов используется метод SendRequestAsync.

### EBCEYS.RabbitMQ.Server.MappedService.SmartController

Аналогичен `RabbitMQControllerBase`, только внутри себя содержит "сервер", принимающий сообщения из брокера.

Пример использования:

```cs
private static RabbitMQConfigurationBuilder? configBuilder;
public static void Main(string[] args)
{
    configBuilder = new();
    configBuilder.AddConnectionFactory(new()
    {
        HostName = "Kuznetsovy-Server",
        UserName = "ebcey1",
        Password = "123"
    });
    configBuilder.AddQueueConfiguration(new("ExampleQueue", autoDelete: true));

    Logger logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSmartRabbitMQController<TestController>(configBuilder.Build());
        })
        .UseNLog()
        .ConfigureLogging(log =>
        {
            log.ClearProviders();
            log.AddNLog("nlog.config");
        })
        .Build();

    host.Run();
}

internal class TestController : RabbitMQSmartControllerBase
{
    private readonly ILogger<TestController> logger;

    public TestController(ILogger<TestController> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    [RabbitMQMethod("ExampleMethod")]
    public Task<string> TestMethod2(string a, string b)
    {
        logger.LogInformation("TestMethod2 get command with args: a: {a}\tb: {b}", a, b);
        return Task.FromResult(a + b);
    }
}
```

### EBCEYS.RabbitMQ.Server.MappedService.RabbitMQServer

Реализация consumer-a как IHostedService.

Стоит отметить что в данном случае consumer всегда будет асинхронный.

### EBCEYS.RabbitMQ.Server.MappedService.RabbitMQMappedServer ***!!! УСТАРЕЛО !!!***

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

## Изменения

### v1.5.1

1. Передача заголовка о наличии компрессии в сообщениях от клиента
1. Автоматическое определение наличия компрессии сообщения на уровне контроллера при приеме сообщения

### v1.5.0

1. Добавлена возможность настройки GZip компрессии/декомпрессии отправляемых сообщений.
1. В `CallbackRabbitMQConfiguration` добавлена конфигурация ожидания ответа на запрос.
1. Для `RabbitMQClient` добавлены новые конструкторы, а старые ***УДАЛЕНЫ***.
1. `AddRabbitMQClient` для `IServiceCollection` ***УБРАНА*** старая реализация с указанием таймаута через параметр.
   Таймаут теперь указывается в `CallbackRabbitMQConfiguration`.
1. Добавлены описания для публичных методов.
1. Исправлена ошибка, когда `RabbitMQSmartController` не обрабатывал сообщения в выбранной кодировке, а только в UTF-8.
1. Добавлена возможность добавлять в `IServiceColletion` свою реализацию `RabbitMQClient`
1. Добавлена возможность добавлять в `IServiceColletion` свою реализацию `RabbitMQServer`

### v1.4.2

1. Исправлена передача исключений.
1. Добавлена конфигурация старта сервисов.
1. Добавлена опция для клиента, не выбрасывать серверные исключения из ответа на запрос.
1. Логи отправки и приема сообщений переведены на уровень *Trace*.

### v1.4.1

1. Добавлена конфигурация QoS [опционально] при старте клиента и сервера.
1. Добавлен вариант передачи исключений. Для этого следует использовать `RabbitMQRequestProcessingException` внутри
   метода контроллера. Если клиент получит ответ с подобным исключением, то выкинет его в методе `SendRequestAsync`.
1. В конфигурацию `RabbitMQConfiguration` добавлена возможность передачи кодировки. По умолчанию используется UTF-8.

### v1.4.0

Фичи:

1. Библиотека переведена на 8-ой дотнет.
2. Обновлены используемые пакеты.
3. Инициализация подключения, создание канала и консумера вынесены в старт сервиса
   `IHostedService.StartAsync(CancellationToken cancellationToken)`.
4. Добавлены поддерживаемые типы обменников как enum-ы (опционально, можно передавать по старому как строку).
5. В конфигурацию добавлены параметры установки канала.
6. В репозиторий добавлены примеры использования в докере. Через сервис ExampleDockerClient можно отправить сообщения и
   запросы на сервис ExampleDockerServer.
7. При создании `RabbitMQSmartController` теперь берется не первый конструктор, а первый подходящий по сервисам в
   `IServiceProvider`. Если ни один конструктор не подошел, то пытаемся создать безе парамметров.
8. Добавлена поддержка topic обменников.
9. Консумеры AutoAck.
10. Добавлена конфигурация для callback обменника и очереди.

Фиксы:

1. Исправлена ошибка, когда очередь не привязывалась к обменнику.

### v1.3.1

1. Методы SendMessage и SendRequest у RabbitMQClient-a сделаны вирутальными чтобы их можно было замокать.
2. Инициализация подключения к RabbitMQ теперь будет происходить на стадии запуска сервиса.

### v1.3.0

1. Добавлен новый тип контроллера `RabbitMQSmartController`.
   Данный контроллер содержит в себе сервер, принимающий сообщения из брокера сообщений и вызывающий методы, указанные
   внутри сообщения.

### v1.2.0

1. Так как использование Microsoft.Text.Json вызывало ошибки в работе контроллера - было принято решение перейти на
   Newtownsoft.Json.
2. В случае если возникли ошибки во время обработки сообщения/запроса, сообщение будет приниматься.

### v1.1.6

1. Исправлена ошибка, когда, при атрибутах метода string, некорректно парсились параметры запроса в контроллере.

### v1.1.5

1. Добавил простые конструкторы для конфигурации.

### v1.1.4

1. В ServiceCollectionExtensions добавлен метод для добавления RabbitMQClient-a.

### v1.1.3

1. Переделаны контроллеры. Теперь они scoped.
2. Убран метод FixRabbitMQControllers для IServiceCollection

### v1.1.2

1. Исправлена ошибка, когда при использовании метода AddRabbitMQController, MappedServer мог не видеть контроллеры.

### v1.1.1 Большая переработка проекта

Фичи:

1. Добавил проекты с примерами работы клиента и сервера;
2. Добавил базовый интерфейс для контроллера;
3. Добавил методы расширения IServiceCollection, через который идет настройка сервера и контроллеров;
4. RabbitMQMappedServer теперь не требует сервиса самого сервера, а инициирует его сам.
5. Убрал кучу лишних конструкторов;
6. Добавил дебаг логирование в сервисы;

Фиксы:

1. Исправлена ошибка, когда таймаут ожидания ответа на запрос учитывался в миллисекундах.
2. Исправлена ошибка, когда не стартовал ресивер, если в настройках не указать, что он асинхронный;
3. Исправил ошибку, когда при отправке ответа на запрос, MappedServer выдавал исключение.

### v1.0.4

1. Изменил алгоритм работы при остановке сервиса. Теперь только закрывает соединение.
2. Добавил реализацию клиента (EBCEYS.RabbitMQ.Client). Через него возможна отправка сообщений и запросов на
   контроллеры.

### v1.0.3

1. Переименовал атрибут для методов RabbitMQ (RabbitMQMethodName -> RabbitMQMethod).
2. Исправил недоработку когда для парсинга сообщения в ресивере не использовались SerializerOptions.
3. Добавил тесты для тестирования в своей среде (во время CI/CD запускаться не будут).

## Планы на развитие

1. Дебаг в зависимости от найденных ошибок / отзывов.
1. Добавить Stream обмен.