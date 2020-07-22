using System;
using System.Linq;
using Viki.LoadRunner.Engine.Aggregators;
using Viki.LoadRunner.Engine.Aggregators.Dimensions;
using Viki.LoadRunner.Engine.Aggregators.Metrics;
using Viki.LoadRunner.Engine.Analytics;
using Viki.LoadRunner.Engine.Core.Factory;
using Viki.LoadRunner.Engine.Core.Scenario;
using Viki.LoadRunner.Engine.Strategies;
using Viki.LoadRunner.Engine.Strategies.Custom.Strategies.Limit;
using Viki.LoadRunner.Engine.Strategies.Custom.Strategies.Threading;
using Viki.LoadRunner.Engine.Strategies.Extensions;
using Viki.LoadRunner.Engine.Validators;
using Viki.LoadRunner.Tools.Extensions;

namespace HttpMockSlim.LoadTest
{
    class Program
    {
        static void Main()
        {
            HttpMock server = new HttpMock();
            server.Add((request, response) =>
            {
                response.SetBody(request.Body);
            });

            server.Start();


            HistogramAggregator aggregator = new HistogramAggregator()
                .Add(new TimeDimension(TimeSpan.FromSeconds(5)))
                .Add(new FuncMetric<int>("Thread Count", 0, (i, result) => Math.Max(i, result.CreatedThreads)))
                .Add(new CountMetric(Checkpoint.NotMeassuredCheckpoints))
                .Add(
                    new PercentileMetric(0.99, 0.9999, 1),
                    // , => . due to ASCII table generator limitations
                    row => row.Select(v => new Val(v.Key.Replace(",","."), v.Value))
                )
                .Add(
                    new TransactionsPerSecMetric(),
                    // converting to int (removing , by removing fraction) due to ASCII table generator limitations
                    row => row.Select(v => new Val(v.Key, Convert.ToInt32(v.Value))) 
                )
                .Add(new ErrorCountMetric(false));

            StrategyBuilder builder = new StrategyBuilder()
                .SetScenario<Scenario>()
                .SetLimit(new TimeLimit(TimeSpan.FromSeconds(30)))
                .SetThreading(new IncrementalThreadCount(4, TimeSpan.FromSeconds(15), 4))
                .SetAggregator(aggregator);

            //builder.BuildUi(new ScenarioValidator(new ScenarioFactory(typeof(Scenario)))).Run();
            builder.Build().Run();

            object[] resultsObj = aggregator.BuildResultsObjects().ToArray();
            string results = String.Join(Environment.NewLine, resultsObj.SerializeToCsv());
            
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

i9 9900K
+----------+------------------+----------------+----------------+-----------------+------+
| Time (s) | Count: Iteration | 95%: Iteration | 99%: Iteration | 100%: Iteration | TPS  |
+----------+------------------+----------------+----------------+-----------------+------+
|        0 |            15752 |              1 |              2 |              38 | 7883 |
|        2 |            17004 |              1 |              2 |              12 | 8501 |
|        4 |            17069 |              1 |              2 |              11 | 8533 |
+----------+------------------+----------------+----------------+-----------------+------+

i9 9900K, but with redesigned scenario:
 * ditched custom http client, and used HttpClient as it comes with .NET
   - HttpClient while overall slower performance, it does however produce more consistent times.
 * Ditched 95% percentile, added 99.99% percentile
 * Increased test scenario runtime to give space for adding:
   - Bigger group by time period - 5s instead of 2s
   - Run test with 4 and 8 threads.
+----------+--------------+------------------+----------------+-------------------+-----------------+------+
| Time (s) | Thread Count | Count: Iteration | 99%: Iteration | 99.99%: Iteration | 100%: Iteration | TPS  |
+----------+--------------+------------------+----------------+-------------------+-----------------+------+
|        0 |            4 |            32538 |              2 |                21 |              25 | 6510 |
|        5 |            4 |            33548 |              2 |                 6 |               8 | 6709 |
|       10 |            4 |            33599 |              2 |                 6 |               7 | 6719 |
|       15 |            8 |            32950 |              7 |                10 |              16 | 6590 |
|       20 |            8 |            33064 |              7 |                14 |              20 | 6609 |
|       25 |            8 |            33078 |              7 |                15 |              16 | 6613 |
+----------+--------------+------------------+----------------+-------------------+-----------------+------+

https://ozh.github.io/ascii-tables/ (it doesn't parse "complex_quoted" types)
*/
