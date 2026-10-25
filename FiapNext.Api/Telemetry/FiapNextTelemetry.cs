using System.Diagnostics;
using System.Diagnostics.Metrics;
using FiapNext.Application.Common.Metrics;

namespace FiapNext.Api.Telemetry;

public class FiapNextTelemetry : IFiapNextMetrics
{
    public const string ActivitySourceName = "FiapNext.Api";
    public const string MeterName = "FiapNext.Metrics";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0.0");
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    private readonly Counter<long> _trabalhosSubmetidosCounter;

    public FiapNextTelemetry()
    {
        _trabalhosSubmetidosCounter = Meter.CreateCounter<long>(
            name: "fiap_next.trabalhos.submetidos_count",
            unit: "{trabalho}",
            description: "Total de trabalhos/projetos submetidos no festival FIAP NEXT");
    }

    public void RecordTrabalhoSubmetido(string categoria)
    {
        _trabalhosSubmetidosCounter.Add(1, new KeyValuePair<string, object?>("categoria", categoria));
    }
}
