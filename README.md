# SslRedirect

ASP.NET Core middleware and MVC extension for redirecting requests to HTTPS.

## Installation

```
Install-Package AspNetCore.SslRedirect
```

## Setup

```csharp
public class Startup {

    public void ConfigureServices(IServiceCollection services) {
        services.AddSslRedirect();
        ...
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
        app.UseSslRedirect();
        ...
    }

}
```

## Secure requests

Use `ISslPolicy` elements or MVC filters to enforce SSL communication:

### Every request

```csharp
services.AddSslRedirect(options => options.Policies.RedirectAll());
```

Upgrades every unsecured request handled by the web application.

### Specific paths

```csharp
services.AddSslRedirect(options =>
    options.Policies
        .RedirectPath("/SecurePath/*.html")
        .RedirectPath("/**/api/Admin/*")
);
```

Upgrades unsecured requests to paths defined by the glob patterns.

### MVC controllers and actions

```csharp
[RequireSsl]
[Route("api/[controller]")]
public class AdministrationController : Controller {

    ...

}
```

Upgrades unsecured requests invoking any controller action.

```csharp
[Route("api/[controller]")]
public class UserController : Controller {

    [HttpGet({id:int})]
    public IActionResult GetById(int id) { ... }

    [RequireSsl]
    [HttpPost("/auth")]
    public IActionResult Authenticate([FromForm]string user, [FromForm]string password) { ... }

}
```

Upgrades unsecured requests invoking action `Authenticate`.

### Custom policies

1) Implement `ISslPolicy`
   ```csharp
   public class SslRemotePolicy : ISslPolicy {
   
       public Task<bool> Accept(HttpContext context) =>
           Task.FromResult(
               context.Request.Host.Host != "localhost" &&
               context.Request.Host.Host != "127.0.0.1"
           );
   
   }
   ```
2) Add your policy
   ```csharp
   public class Startup {
   
       public void ConfigureServices(IServiceCollection services) {
           services.AddSslRedirect(options => options.Policies.Add(new SslRemotePolicy());
           ...
       }
   
       public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
           app.UseSslRedirect();
           ...
       }
   
   }
   ```

## Options

* **SslPort** - The HTTPS port number

  ```csharp
  options.SslPort = Environment.IsDevelopment() ? 44300 : 443;
  ```
* **Method** - The HTTP method used for redirecting requests. See
  [RFC 7231, Section 6.4](https://tools.ietf.org/html/rfc7231#section-6.4) and
  [RFC 7238](https://tools.ietf.org/html/rfc7238)
  
  ```csharp
  options.Method = HttpRedirectMethod.TemporaryRedirect;
  ```
* **HstsHeader** - The HSTS header information

  ```csharp
  options.HstsHeader.MaxAge = TimeSpan.FromMonths(1);
  options.HstsHeader.IncludeSubDomains = true;
  ```
  The middleware will automatically add a [HSTS header](https://tools.ietf.org/html/rfc6797) unless
  `options.HstsHeader` is `null`.
* **Policies** - The collection of policies for upgrading unsecured requests.
