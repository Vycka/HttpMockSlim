using System;
using Newtonsoft.Json;
using Viki.LoadRunner.Engine;
using Viki.LoadRunner.Engine.Aggregators;
using Viki.LoadRunner.Engine.Aggregators.Dimensions;
using Viki.LoadRunner.Engine.Aggregators.Metrics;
using Viki.LoadRunner.Engine.Executor.Context;
using Viki.LoadRunner.Engine.Settings;
using Viki.LoadRunner.Engine.Strategies.Limit;
using Viki.LoadRunner.Engine.Strategies.Threading;

namespace HttpMockSlim.LoadTest
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpMock server = new HttpMock();
            server.Add((request, response) => response.SetBody(request.Body));

            server.Start();

            LoadRunnerSettings settings = new LoadRunnerSettings()
                .SetScenario<Scenario>()
                .SetLimits(new TimeLimit(TimeSpan.FromSeconds(6)))
                .SetThreading(new FixedThreadCount(4));

            HistogramAggregator aggregator = new HistogramAggregator()
                .Add(new TimeDimension(TimeSpan.FromSeconds(2)))
                .Add(new CountMetric(Checkpoint.Names.Setup, Checkpoint.Names.TearDown))
                .Add(new PercentileMetric(new [] { 0.95, 0.99, 1 }, new []{ Checkpoint.Names.Setup, Checkpoint.Names.TearDown, Checkpoint.Names.IterationStart } ))
                .Add(new TransactionsPerSecMetric());

            LoadRunnerEngine engine = LoadRunnerEngine.Create(settings, aggregator);
            engine.Run();

            string results = JsonConvert.SerializeObject(aggregator.BuildResultsObjects(), Formatting.Indented);

            Console.WriteLine(results);

            Console.ReadLine();
        }
    }
}

/*
i7 4600U
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+--------------------|
| Time (s) | Count: ITERATION_START | Count: ITERATION_END | 95%: ITERATION_END | 99%: ITERATION_END | 100%: ITERATION_END |        TPS         |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+--------------------|
|    0     |          6113          |         6113         |         2          |         3          |         40          | 3062.160096722783  |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+--------------------|
|    2     |          6577          |         6577         |         2          |         3          |          4          | 3284.7397941206805 |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+--------------------|
|    4     |          3382          |         3382         |         2          |         3          |          4          | 3249.3139403158098 |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+--------------------|


i5 4670
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+-------------------|
| Time (s) | Count: ITERATION_START | Count: ITERATION_END | 95%: ITERATION_END | 99%: ITERATION_END | 100%: ITERATION_END |        TPS        |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+-------------------|
|    0     |         10655          |        10655         |         1          |         2          |         28          | 5335.683337534778 |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+-------------------|
|    2     |         11114          |        11114         |         1          |         2          |         11          | 5554.857491465542 |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+-------------------|
|    4     |         11656          |        11656         |         1          |         2          |          5          | 5828.311523250918 |
|----------+------------------------+----------------------+--------------------+--------------------+---------------------+-------------------|

 */
