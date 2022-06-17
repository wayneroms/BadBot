using FtxApi;
using SimpleFX;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("FTX", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://ftx.com/");
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient("SimpleFX", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://rest.simplefx.com/");
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<FtxConfig>(builder.Configuration.GetSection("FTX"));
builder.Services.Configure<SimpleFxConfig>(builder.Configuration.GetSection("SimpleFX"));

builder.Services.AddScoped<FtxRestApi, FtxRestApi>();
builder.Services.AddScoped<SimpleFxApi, SimpleFxApi>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
