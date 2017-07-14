using System;
using HttpMockSlim.LoadTest.Client;
using HttpMockSlim.LoadTest.Client.Data;
using HttpMockSlim.LoadTest.Client.Enums;
using Viki.LoadRunner.Engine;
using Viki.LoadRunner.Engine.Executor.Context;

namespace HttpMockSlim.LoadTest
{
    public class Scenario : ILoadTestScenario
    {
        private static readonly byte[] _a2000Gzipped = Convert.FromBase64String("H4sIAAAAAAAA/3N0HAWjYBSMglEwCkbBUAcAVwDM/9AHAAA=");
        private static readonly SimpleWebRequest _client = new SimpleWebRequest();


        public void ScenarioSetup(ITestContext context)
        {
        }

        public void IterationSetup(ITestContext context)
        {
        }

        public void ExecuteScenario(ITestContext context)
        {
            RequestResult response = _client.Execute(
                "http://localhost:8080/",
                "/",
                RequestMethod.POST,
                SubmitRequestType.OCTET_STREAM,
                _a2000Gzipped,
                new Header("Content-Encoding", "gzip")
            );

            if (response.Response.Data.Length != 2000)
                throw new Exception(response.Response.Data);
        }

        public void IterationTearDown(ITestContext context)
        {
        }

        public void ScenarioTearDown(ITestContext context)
        {
        }
    }
}