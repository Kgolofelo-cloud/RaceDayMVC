var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Controllers and Views

builder.Services.AddControllersWithViews();

// 2. Configure HttpClient with a Cookie Container to remember API sessions
var cookieContainer = new System.Net.CookieContainer();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7081/api/v1/"); // Keep your correct port here
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
    CookieContainer = cookieContainer,
    UseCookies = true
});

// 3. Add Session Management for Role-Based Navigation

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>

{

    options.IdleTimeout = TimeSpan.FromHours(2);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// 4. Enable Session (Must be before Authorization)

app.UseSession();

app.UseAuthorization();

// 5. Default route pointing to the Auth Controller we will build next

app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
