// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var builder = ConsoleApp.CreateBuilder(args);
builder.ConfigureServices(v =>
{

});

var app = builder.Build();

await app.RunAsync();
