using System.Reflection;
using EBCEYS.RabbitMQ.Server.MappedService.Data;

namespace EBCEYS.RabbitMQ.Server.MappedService.SmartController;

/// <summary>
///     The <see cref="CustomParameterProcessingOptions" /> record.
/// </summary>
/// <param name="ParameterSelection">The parameter selection predicate.</param>
/// <param name="ParameterProcessing">The parameter processing delegate.</param>
/// <remarks>
///     <paramref name="ParameterProcessing" /> calls only if <paramref name="ParameterSelection" /> predicate pass.
/// </remarks>
public record CustomParameterProcessingOptions(
    Predicate<ParameterInfo> ParameterSelection,
    Func<int, MethodInfo, ParameterInfo, RabbitMQRequestData?, object> ParameterProcessing);