// 

using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

public class Kicker
{

    static List<string> teamNames = new List<string>();
    static List<string> playerNames = new List<string>();


    static List<Team> teams = new List<Team>();
    static List<Player> players = new List<Player>();
    static List<Match> matches = new List<Match>();

    // Allowed characters for team and player names (for now, these should be enough)
    static string ID_REGEX = "[0-9a-zA-Z]+";

    public static void Main()
    {
        // A test string to mess around. This won't remain in the final version
        string testString = "";
        // I know, List isn't the best choice, but right now I just want
        // it to work somehow, no matter how well.


        new WebHostBuilder()
            .UseKestrel()
            .Configure(app =>
                app.Run(async context =>
                {
                    var req = context.Request;
                    var res = context.Response;

                    // body will contain the HTML code to be presented
                    // on the respective page
                    byte[] body;
                    try
                    {
                        var path = req.Path.ToString();
                        if (path.StartsWith("/teams"))
                        {
                            await TeamsEndpoint(req, res);
                        }
                        else if (path.StartsWith("/players"))
                        {
                            // Until I have implemented the endpoints,
                            // this case will return a teapot.

                            // await PlayersEndpoint(req, res);
                            res.StatusCode = 418;
                            body = Encoding.UTF8.GetBytes("<h1>This option has not yet been implemented.</h1>");
                            res.ContentType = "text/html; charset=utf-8";
                            res.ContentLength = body.LongLength;
                            await res.Body.WriteAsync(body, 0, body.Length);
                        }
                        else if (req.Path == "/")
                        {
                            // If there is nothing more in the path, then the
                            // index.html will be used. The main purpose is to
                            // mess around again and learn how everything works.
                            res.StatusCode = 200;
                            body = System.IO.File.ReadAllBytes("index.html");
                            res.ContentType = "text/html; charset=utf-8";
                            res.ContentLength = body.LongLength;
                            await res.Body.WriteAsync(body, 0, body.Length);
                        }
                        else if (req.Path == "/test.js")
                        {
                            // Allow external Javascript files
                            res.StatusCode = 200;
                            body = System.IO.File.ReadAllBytes("test.js");
                            res.ContentType = "text/javascript; charset=utf-8";
                            res.ContentLength = body.LongLength;
                            await res.Body.WriteAsync(body, 0, body.Length);
                        }
                        else
                        {
                            // 404 if the path doesn't exist
                            res.StatusCode = 404;
                            body = Encoding.UTF8.GetBytes("<h1>Not Found</h1>");
                            res.ContentType = "text/html; charset=utf-8";
                            res.ContentLength = body.LongLength;
                            await res.Body.WriteAsync(body, 0, body.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        res.StatusCode = 500;
                    }
                })
            )
            .Build()
            .Run();
    }

    // /teams
    static async Task TeamsEndpoint(HttpRequest req, HttpResponse res)
    {
        var segments = req.Path.ToString().Split("/");
        if (new Regex($"^/teams/{ID_REGEX}$").IsMatch(req.Path))
        {
            string name = segments[segments.Length - 1];
            if (req.Method == "POST")
            {
                PostTeam(name, req, res);
                byte[] body = Encoding.UTF8.GetBytes("<h1>here should be a layout.</h1>");
                res.ContentType = "text/html; charset=utf-8";
                res.ContentLength = body.LongLength;
                await res.Body.WriteAsync(body, 0, body.Length);
            }
            else
            {
                // For now, I will only allow POST.
                res.StatusCode = 405;
            }
        }
        else
        {
            res.StatusCode = 404;
        }
    }

    // POST /teams/$teamId
    static void PostTeam(string name, HttpRequest req, HttpResponse res)
    {

        // Old implementation with a team pool.
        // I will implement it anew.

        if (teamNames.Contains(name))
        {
            res.StatusCode = 409;
        }
        else
        {
            teams.Add(new Team(name));
            teamNames.Add(name);
            res.StatusCode = 201;
        }
    }


    // I have an identical function to find a player inside
    // the class Team. There is probably a better solution.
    static Team FindTeam(string teamName, List<Team> teamPool)
    {
        Team result = teamPool.Find(
            delegate (Team t)
            {
                return t.name == teamName;
            }
        );
        return result;
    }

    // I have copied the following function entirely from a friend of mine.
    static Task WriteCsv(string csv, HttpResponse res)
    {
        var body = Encoding.UTF8.GetBytes(csv);
        res.StatusCode = 200;
        res.ContentType = "text/csv; charset=utf-8";
        res.ContentLength = body.LongLength;
        return res.Body.WriteAsync(body, 0, body.Length);
    }
}

public class Team
{
    public string name;     // name must be unique. Hence no id
                            // public int score;    // I probably won't need a score, since
                            // it's no heavy task to just calculate them,
                            // and this allows for more flexibility.
    public List<Player> members = new List<Player>();
    // The following function only adds completely new players.
    // A hybrid which adds new and also preexisting players must be implemented.

    //Constructor
    public Team(string teamName, params string[] nms)
    {
        name = teamName;
        foreach (string value in nms)
        {
            members.Add(new Player(value));
            // Console.WriteLine("bla");
        }
    }

    // Add
    public int addPlayer(Guid playerid, List<Player> playerPool)
    {
        Player newMember = findPlayer(playerid, playerPool);
        if (newMember != null)
        {
            members.Add(newMember);
            return 0;
        }
        else return 1;
    }

    //Remove
    public int removeMember(Guid playerid)
    {
        Player goer = findPlayer(playerid, members);
        if (goer != null)
        {
            members.Remove(goer);
            return 0;
        }
        else return 1;
    }

    // Helper function to find the player to a corresponding id
    // Maybe it should be put into another class. But that's something
    // I will worry about later.
    public Player findPlayer(Guid playerid, List<Player> playerPool)
    {
        Player result = playerPool.Find(
            delegate (Player p)
            {
                return p.id == playerid;
            }
        );
        return result;
    }
}

public class Player
{
    // Players can have identical names. Hence, they need id's.
    public Guid id;
    public string name;

    // Constructor
    public Player(string nm)
    {
        name = nm;
        id = Guid.NewGuid();
    }

    // Function to return the id. id will likely become private.
    public Guid ID()
    {
        return id;
    }
}

// Store matches instead of points. The number is unlikely to become
// large. Hence, all data can be stored, and the points can be
// computed later.
public class Match
{
    // two teams
    Team t1, t2;

    // respective goals
    int goalst1, goalst2;

    // Matches do not have names and hence need id's.
    public Guid id;

    // Constructor
    // No need for edit functions. Just rewrite the parameters manually.
    public Match(Team ta, Team tb, int goalsta, int goalstb)
    {
        id = Guid.NewGuid();
        t1 = ta;
        t2 = tb;
        goalst1 = goalsta;
        goalst2 = goalstb;
    }
}
