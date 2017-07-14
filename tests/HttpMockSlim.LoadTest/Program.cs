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
                .SetLimits(new TimeLimit(TimeSpan.FromSeconds(5)))
                .SetThreading(new FixedThreadCount(4));

            HistogramAggregator aggregator = new HistogramAggregator()
                .Add(new TimeDimension(TimeSpan.FromSeconds(1)) {TimeSelector = result => result.IterationStarted})
                .Add(new CountMetric(Checkpoint.Names.Setup, Checkpoint.Names.TearDown))
                .Add(new PercentileMetric(new [] { 0.95, 0.99, 1 }, new []{ Checkpoint.Names.Setup, Checkpoint.Names.TearDown, Checkpoint.Names.IterationStart } ));

            LoadRunnerEngine engine = LoadRunnerEngine.Create(settings, aggregator);
            engine.Run();

            string results = JsonConvert.SerializeObject(aggregator.BuildResultsObjects(), Formatting.Indented);

            Console.WriteLine(results);

            Console.ReadLine();
        }
    }
}

/*
[
  {
    "Time (s)": "0",
    "Count: ITERATION_START": 2581,
    "Count: ITERATION_END": 2581,
    "95%: ITERATION_END": 3,
    "99%: ITERATION_END": 5,
    "100%: ITERATION_END": 59
  },
  {
    "Time (s)": "1",
    "Count: ITERATION_START": 2659,
    "Count: ITERATION_END": 2659,
    "95%: ITERATION_END": 3,
    "99%: ITERATION_END": 5,
    "100%: ITERATION_END": 26
  },
  {
    "Time (s)": "2",
    "Count: ITERATION_START": 3294,
    "Count: ITERATION_END": 3294,
    "95%: ITERATION_END": 2,
    "99%: ITERATION_END": 3,
    "100%: ITERATION_END": 3
  },
  {
    "Time (s)": "3",
    "Count: ITERATION_START": 3324,
    "Count: ITERATION_END": 3324,
    "95%: ITERATION_END": 2,
    "99%: ITERATION_END": 3,
    "100%: ITERATION_END": 4
  },
  {
    "Time (s)": "4",
    "Count: ITERATION_START": 3307,
    "Count: ITERATION_END": 3307,
    "95%: ITERATION_END": 2,
    "99%: ITERATION_END": 3,
    "100%: ITERATION_END": 3
  }
]
 */
