using System;
using System.Linq;
using Viki.LoadRunner.Engine.Aggregators;
using Viki.LoadRunner.Engine.Aggregators.Dimensions;
using Viki.LoadRunner.Engine.Aggregators.Metrics;
using Viki.LoadRunner.Engine.Analytics;
using Viki.LoadRunner.Engine.Analytics.Metrics;
using Viki.LoadRunner.Engine.Core.Collector.Interfaces;
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
            // Http server setup

            HttpMock server = new HttpMock();
            server.Add((request, response) =>
            {
                response.SetBody(request.Body);
            });

            server.Start();


            // Load-Test 

            HistogramAggregator summaryAggregator = new HistogramAggregator()
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

            HistogramAggregator countPerThreadAggregator = new HistogramAggregator()
                .Add(new TimeDimension(TimeSpan.FromSeconds(5)))
                .Add(new SubDimension<IResult>(
                        new FuncDimension("", r => r.ThreadId.ToString()),
                        (dimValue, metricName) => $"Thread {dimValue} ({metricName})",
                        new CountMetric<IResult>()
                    ),
                    r => r.OrderBy(v => v.Key)
                );
            

            StrategyBuilder builder = new StrategyBuilder()
                .SetScenario<Scenario>()
                .SetLimit(new TimeLimit(TimeSpan.FromSeconds(30)))
                .SetThreading(new IncrementalThreadCount(4, TimeSpan.FromSeconds(15), 4))
                .SetAggregator(summaryAggregator, countPerThreadAggregator);

            //builder.BuildUi(new ScenarioValidator(new ScenarioFactory(typeof(Scenario)))).Run();
            builder.Build().Run();

            PrintResults(summaryAggregator);
            PrintResults(countPerThreadAggregator);

            Console.ReadLine();
        }

        private static void PrintResults(HistogramAggregator histogram)
        {
            object[] resultsObj = histogram.BuildResultsObjects().ToArray();
            string results = String.Join(Environment.NewLine, resultsObj.SerializeToCsv());

            Console.WriteLine(results);
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
|        0 |            4 |            32336 |              2 |                21 |              25 | 6471 |
|        5 |            4 |            33341 |              2 |                 6 |               7 | 6667 |
|       10 |            4 |            33749 |              2 |                 6 |               7 | 6749 |
|       15 |            8 |            33443 |              7 |                10 |              12 | 6688 |
|       20 |            8 |            33268 |              7 |                11 |              13 | 6650 |
|       25 |            8 |            33372 |              7 |                14 |              17 | 6673 |
+----------+--------------+------------------+----------------+-------------------+-----------------+------+


+----------+------------------+------------------+------------------+------------------+------------------+------------------+------------------+------------------+
| Time (s) | Thread 0 (Count) | Thread 1 (Count) | Thread 2 (Count) | Thread 3 (Count) | Thread 4 (Count) | Thread 5 (Count) | Thread 6 (Count) | Thread 7 (Count) |
+----------+------------------+------------------+------------------+------------------+------------------+------------------+------------------+------------------+
|        0 |             8105 |             8133 |             8074 |             8024 |                  |                  |                  |                  | 
|        5 |             8383 |             8265 |             8326 |             8367 |                  |                  |                  |                  |
|       10 |             8477 |             8447 |             8421 |             8404 |                  |                  |                  |                  |
|       15 |             4187 |             4258 |             4266 |             4154 |             4196 |             4092 |             4182 |             4108 |
|       20 |             4154 |             4190 |             4201 |             4193 |             4178 |             4092 |             4155 |             4105 |
|       25 |             4184 |             4070 |             4199 |             4213 |             4194 |             4213 |             4250 |             4049 |
+----------+------------------+------------------+------------------+------------------+------------------+------------------+------------------+------------------+


https://ozh.github.io/ascii-tables/ (it doesn't parse "complex_quoted" types)
*/
