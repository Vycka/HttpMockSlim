# HttpMockSlim

Slim and simple concurrency-supported Http Server Mock.
Still under slow development :)

## NuGet
https://www.nuget.org/packages/Viki.HttpMockSlim/0.0.1-beta

## Playground Program.cs
```cs
static void Main()
{
    HttpMock httpMock = new HttpMock();
    httpMock.Start("http://localhost:50000/");

    httpMock.Add("GET", "/test", (request, response) =>
    {
        Thread.Sleep(3000);
        response.Body($"{request}\r\nFoo!");
    });

    httpMock.Add("GET", "/", (request, response) => response.Body($"{request}\r\nWoah!"));

    httpMock.Add((request, response) => response.Body($"{request}\r\rMini Wild!"));

    httpMock.Add("GET", "/test", (request, response) => response.Body($"{request}\r\nBOOO!"));

    Console.WriteLine("Enter to exit...");
    Console.ReadLine();
}
```
