# EBCEYS.RabbitMQ

## ��������

���������� ��� ������ � �������� ��������� RabbitMQ.

### RabbitMQ.Server.Service

���������� consumer-a ��� IHostedService.

### RabbitMQ.Server.MapedService

������� ���������� ������� consumer-a ����� ����������� (�-�� ControllerBase ��� http).

### ������ ��� ���������
```json
public class RabbitMQRequestData
    {
        public object[]? Params { get; set; }
        public string? Method { get; set; }
    }
```