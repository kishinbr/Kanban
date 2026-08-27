using KanbanApp.Data;
using KanbanApp.Data.Repositorios;
var builder = WebApplication.CreateBuilder(args);
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ConexaoBanco>();
builder.Services.AddScoped<UsuarioRepositorio>();
builder.Services.AddScoped<QuadroRepositorio>();
builder.Services.AddScoped<ColunaRepositorio>();
builder.Services.AddScoped<CartaoRepositorio>();

builder.Services.AddAuthentication("CookieKanban")
    .AddCookie("CookieKanban", options =>
    {
        options.LoginPath = "/Conta/Login";
        options.AccessDeniedPath = "/Conta/Login";
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Conta}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
