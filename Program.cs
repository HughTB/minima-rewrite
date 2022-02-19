using System.Web;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

string domain = @"https://minima.hughtb.uk/";
string indexFile = "index.html";
string index = "";

string dictPath = "dict.json";
Dictionary<string, string>? urlDict = new Dictionary<string, string>();
int addedSinceSave = 0;

char[] idCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_' };
int idLength = 8;

try
{
    using (StreamReader sr = new StreamReader(indexFile))
    {
        index = sr.ReadToEnd();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.MapGet("/", () =>
{
    return Results.Content(index, "text/html");
});

app.MapGet("/{id}", (string id) =>
{
    try
    {
        if (urlDict.ContainsKey(id))
        {
            return Results.Redirect(HttpUtility.UrlDecode(urlDict[id]));
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.BadRequest();
    }
});

app.MapPost("/{url}", (string url) =>
{
    try
    {
        url = (HttpUtility.UrlDecode(url).StartsWith("http://") || HttpUtility.UrlDecode(url).StartsWith("https://")) ? url : $"http://{url}";

        string id = RandomID();

        if (urlDict.ContainsValue(url))
        {
            id = urlDict.FirstOrDefault(x => x.Value == url).Key;
        }
        else
        {
            while (urlDict.ContainsKey(id))
            {
                id = RandomID();
            }

            urlDict.Add(id, url);
            Console.WriteLine($"{id} relates to {url}");

            addedSinceSave++;

            if (addedSinceSave >= 5)
            {
                SaveDict(urlDict, dictPath);
            }
        }

        return Results.Created($"/{id}", $"{domain}{id}");
    } 
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.BadRequest();
    }
});

if (LoadDict(out urlDict, dictPath))
{
    app.Run();
}
else
{
    Console.WriteLine($"Load failed, check {dictPath}");
}

string RandomID()
{
    Random random = new Random();
    string id = "";

    for (int i = 0; i < idLength; i++)
    {
        id = id + idCharacters[random.Next(0, idCharacters.Length)];
    }

    return id;
}

bool LoadDict(out Dictionary<string, string>? dict, string filepath)
{
    Console.WriteLine("Loading dictionary...");

    if (!File.Exists(filepath))
    {
        dict = new Dictionary<string, string>();
        SaveDict(dict, filepath);
        return true;
    }

    try
    {
        using (StreamReader sr = new StreamReader(filepath))
        {
            dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
            Console.WriteLine("Load successful");
            return true;
        }
    } 
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        dict = new Dictionary<string, string>();
        return false;
    }
}

bool SaveDict(Dictionary<string, string>? dict, string filepath)
{
    Console.WriteLine("Saving dictionary...");

    try
    {
        using (StreamWriter sw = new StreamWriter(filepath))
        {
            sw.Write(JsonConvert.SerializeObject(dict, Formatting.Indented));
            Console.WriteLine("Save successful");
            return true;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
}