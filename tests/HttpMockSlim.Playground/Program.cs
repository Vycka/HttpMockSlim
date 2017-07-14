using System;
using System.IO;
using System.Threading;
using HttpMockSlim.Playground.Handlers;

namespace HttpMockSlim.Playground
{
    partial class Program
    {
        private static readonly string _hostUrl = "http://localhost:8080/";

        static void Main()
        {
            // ### Starting the server ###
            HttpMock httpMock = new HttpMock();
            httpMock.Start(_hostUrl);


            // ### Seting up handlers ###
            // ! Server will try to check requests against each handler in registered order, until first handler processes it.
            // --- Mocked swift storage ---
            httpMock.Add(new SwiftAuthMock("/fake-swift/"));
            httpMock.Add(new FakeStorageHandler("/fake-swift/"));



            // --- Simplified & filtered (by HTTP method & path) func handlers ---
            httpMock.Add("GET", "/sleep", (request, response) =>
            {
                Thread.Sleep(2000);
                response.SetBody($"{request}\r\nSlept like a baby!");
            });
            httpMock.Add("GET", "/", (request, response) => response.SetBody($"{request}\r\nThe root is strong with this one!"));



            // --- Other Random examples ---
            // * Another low level example
            httpMock.Add(new LowLevelExample());
            // * This handler will handle all requests, since it doesn't have a filter
            httpMock.Add((request, response) => response.SetBody($"{request}\r\rGotta catch them all!"));
            // * This one will never be activated, because "Gotta catch them all!" will always handle it.
            httpMock.Add("GET", "/will-not-work", (request, response) => response.SetBody($"{request}\r\nBOOO!"));
            

            Console.WriteLine("Enter to exit...");
            Console.ReadLine();
        }
    }
}
