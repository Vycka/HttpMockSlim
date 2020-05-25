# HttpMockSlim

Leightweight Http Server. 
* Created to be used in internal load-tests to mock some http services.

Developed with these requirements in mind:
* Support for concurrent requests.
* No admin-rights necessary (unless binding address requires elevated permissions).
* It's a real http server.
* Easy to mock responses with funcs or with custom IHttpHandlerMock.

Cons:
* It is small, so only simplified handler exists - which supports mocking StatusCode/ContentType/ResponseBody.
* Advanced features require implementing IHttpHandlerMock and handling native c# HttpListenerContext.

## NuGet
https://www.nuget.org/packages/Viki.HttpMockSlim 
* `Install-Package Viki.HttpMockSlim`

## Examples / HttpMockSlim.Playground

### app.config
* Don't forget .NET connection limiting features if want more concurrency
```xml
    <system.net>
        <connectionManagement>
            <add address="*" maxconnection="100" /> <!-- By default afaik its 2 -->
        </connectionManagement>
    </system.net>
```

### Program.cs 
```cs
// ### Starting the server ###
HttpMock httpMock = new HttpMock();
httpMock.Start("http://localhost:8080/");


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
```

### Custom handlers

```cs
public class LowLevelExample : IHttpHandlerMock
{
    // Lets handle all DELETE's
    public bool Handle(HttpListenerContext context)
    {
        if (context.Request.HttpMethod != "DELETE")
            return false;

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain";

        new StreamGenerator(256, 'A').CopyTo(context.Response.OutputStream);

        // One must not forget to close it, as .NET can decide to send the data only after Close().
        context.Response.Close();

        return true;
    }
}

public class SwiftAuthMock : FilteredHandlerBase
{
    private readonly string _fakeSwiftPath;

    public SwiftAuthMock(string fakeSwiftPath) : base("GET", "/auth")
    {
        if (fakeSwiftPath == null)
            throw new ArgumentNullException(nameof(fakeSwiftPath));
                
        _fakeSwiftPath = fakeSwiftPath;
    }

    protected override bool HandleInner(HttpListenerContext context)
    {
        // No idea if this response is valid for Swift RFC. but it works :)

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain";

        context.Response.SendChunked = true;
        context.Response.AddHeader("X-Auth-Token", "fifty_shades_of_mocked_passkey");
        context.Response.AddHeader("X-Storage-Url", $"{context.Request.Url.Scheme}://{context.Request.Url.Authority}{_fakeSwiftPath}");

        context.Response.Close();

        return true;
    }
}

public class FakeStorageHandler : IHttpHandlerMock
{
    protected readonly string PathBase;

    public FakeStorageHandler(string pathBase)
    {
        if (pathBase == null)
            throw new ArgumentNullException(nameof(pathBase));

        PathBase = pathBase;
    }

    public bool Handle(HttpListenerContext context)
    {
        bool handled = false;
        HttpListenerRequest request = context.Request;

        if (request.RawUrl.StartsWith(PathBase, StringComparison.InvariantCulture))
        {
            handled = true;

            switch (request.HttpMethod)
            {
                case "GET":
                    HandleGet(context);
                    break;

                case "PUT":
                    HandlePut(context);
                    break;

                case "DELETE":
                    HandleDelete(context);
                    break;

                default:
                    handled = false;
                    break;
            }
        }

        return handled;
    }

    protected virtual void HandleGet(HttpListenerContext context)
    {
        WriteResponse(context, new StreamGenerator(16, '#'));
    }

    protected virtual void HandlePut(HttpListenerContext context)
    {
        // Don't forget to read all what was sent anyway
        var result = context.Request.InputStream.ReadAllBytes();

        WriteResponse(context, new StreamGenerator(0, 0));
    }

    protected virtual void HandleDelete(HttpListenerContext context)
    {
        WriteResponse(context, new StreamGenerator(0, 0));
    }

    protected static void WriteResponse(HttpListenerContext context, int statusCode)
    {
        HttpListenerResponse response = context.Response;

        response.StatusCode = statusCode;
        response.ContentType = "text/plain";
        response.SendChunked = true;

        response.Close();
    }

    protected static void WriteResponse(HttpListenerContext context, Stream body)
    {
        HttpListenerResponse response = context.Response;

        response.StatusCode = 200;
        response.ContentType = "text/plain";
        response.SendChunked = true;

        body.CopyTo(response.OutputStream);

        response.Close();
    }
}
```
