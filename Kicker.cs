// 

using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

public class Kicker
{
  public static void Main()
  {
    new WebHostBuilder()
        .UseKestrel()
        .Configure(app =>
            app.Run(async context =>
            {
            var req = context.Request;
            var res = context.Response;
            if (req.Path == "/")
            {
                res.StatusCode = 200;
                // var index = File.ReadAllBytes('index.html');
                // byte[] body = Encoding.UTF8.GetBytes("<h1>OK</h1>");
                byte[] body = System.IO.File.ReadAllBytes("index.html");
                res.ContentType = "text/html; charset=utf-8";
                res.ContentLength = body.LongLength;
                await res.Body.WriteAsync(body, 0, body.Length);
            }
            else if (req.Method == "POST"
                && req.Path == "/teams"
                && req.Query.ContainsKey("name"))
            {
                // Generate id for the new team
                Guid id = Guid.NewGuid();

                // Get name from query parameters
                StringValues name;
                req.Query.TryGetValue("name", out name);

                // Format JSON body
                string json = "{" +
                "\"id\":\"" + id.ToString() + "\"," +
                "\"name\":\"" + name.ToString() + "\"" +
                "}";
                byte[] body2 = Encoding.UTF8.GetBytes(json);

                // Return 202 Created
                res.StatusCode = 202;
                res.ContentType = "application/json; charset=utf-8";
                res.ContentLength = body2.LongLength;
                await res.Body.WriteAsync(body2, 0, body2.Length);
            }
            else if (req.Path == "/test.js")
            {
                res.StatusCode = 200;
                byte[] body = System.IO.File.ReadAllBytes("test.js");
                res.ContentType = "text/javascript; charset=utf-8";
                res.ContentLength = body.LongLength;
                await res.Body.WriteAsync(body, 0, body.Length);
            }
            else
            {
                res.StatusCode = 404;
                byte[] body = Encoding.UTF8.GetBytes("<h1>Not Found</h1>");
                res.ContentType = "text/html; charset=utf-8";
                res.ContentLength = body.LongLength;
                await res.Body.WriteAsync(body, 0, body.Length);
            }
            })
        )
        .Build()
        .Run();
  }
}