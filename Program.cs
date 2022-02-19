var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () =>
{

});

app.MapGet("/{id}", (string id) =>
{

});

app.MapPost("/{url}", (string url) =>
{

});

app.Run();