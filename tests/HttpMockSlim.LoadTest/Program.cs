using System;
using System.Linq;
using Newtonsoft.Json;
using Viki.LoadRunner.Engine;
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
using Viki.LoadRunner.Engine.Utils;
using Viki.LoadRunner.Engine.Validators;
using Viki.LoadRunner.Tools.Extensions;

namespace HttpMockSlim.LoadTest
{
    class Program
    {
        static void Main(string[] args)
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
                .Add(new PercentileMetric(0.99, 0.9999, 1))
                .Add(
                    new TransactionsPerSecMetric(),
                    // converting to int due to ASCII table generator limitations
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
|        0 |            4 |            32900 |              2 |                19 |              24 | 6583 |
|        5 |            4 |            33547 |              2 |                 4 |               7 | 6706 |
|       10 |            4 |            33601 |              2 |                 6 |               7 | 6720 |
|       15 |            8 |            33446 |              7 |                14 |              16 | 6690 |
|       20 |            8 |            33020 |              7 |                14 |              16 | 6605 |
|       25 |            8 |            33361 |              7 |                11 |              14 | 6670 |
+----------+--------------+------------------+----------------+-------------------+-----------------+------+


https://ozh.github.io/ascii-tables/ (it doesn't parse "complex_quoted" types)
*/
