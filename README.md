# HttpMockSlim

Slim and simple  Http Server Mock.
Still in development, created to be used in internal load-tests to mock some http services

Developed with these requirements in mind:
* No admin-rights necessary
* No dependencies on other nugets
* Concurrency
* Easy to mock responses with funcs or with custom IHttpHandlerMock

Cons:
* It is small, so only simplified handler exists - which supports mocking StatusCode/ContentType/ResponseBody.
* Advanced features require implementing IHttpHandlerMock and handling native c# HttpListenerContext.

## NuGet
https://www.nuget.org/packages/Viki.HttpMockSlim

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
