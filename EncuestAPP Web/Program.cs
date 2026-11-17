using EncuestAPP_Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<EncuestaPublicaApiClient>(client =>
{
    var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Configure Api:BaseUrl en appsettings.json.");
    if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseAddress) || baseAddress.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException("Api:BaseUrl debe ser una URL HTTPS absoluta.");
    }

    client.BaseAddress = baseAddress;
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
