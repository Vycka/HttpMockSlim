using System;
using System.Net.Http;
using Viki.LoadRunner.Engine.Core.Scenario.Interfaces;

namespace HttpMockSlim.LoadTest
{
    public class Scenario : IScenario
    {
        private static readonly byte[] _a2000Gzipped = Convert.FromBase64String("H4sIAAAAAAAA/3N0HAWjYBSMglEwCkbBUAcAVwDM/9AHAAA=");
        private static readonly HttpClient _client = new HttpClient();


        public void ScenarioSetup(IIteration context)
        {
        }

        public void IterationSetup(IIteration context)
        {
        }

        public void ExecuteScenario(IIteration context)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/"))
            {
                using (request.Content = new ByteArrayContent(_a2000Gzipped))
                {
                    request.Content.Headers.ContentEncoding.Add("gzip");
                    request.Headers.TransferEncodingChunked = true;

                    using (HttpResponseMessage response = _client.SendAsync(request).Result)
                    {
                        string responseBody = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                            throw new Exception(responseBody);
                    }
                }
            }
        }

        public void IterationTearDown(IIteration context)
        {
        }

        public void ScenarioTearDown(IIteration context)
        {
        }
    }
}