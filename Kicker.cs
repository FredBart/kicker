using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

public class Kicker
{
    public static void Main()
    {
        new WebHostBuilder()
            .UseKestrel()
            .Configure(app =>
                app.Run(async context => {
                    var req = context.Request;
                    var res = context.Response;
                    if (req.Path == "/"){
                        res.StatusCode=200;
                    }else{
                        res.StatusCode=404;
                    }
                })
            )
            .Build()
            .Run();
    }
}