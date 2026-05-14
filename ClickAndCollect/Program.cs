using ClickAndCollect.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add the DAL to the services collection
string? connectionString = builder.Configuration.GetConnectionString("default");
builder.Services.AddTransient<IArticleDAL>(aDal => new ArticleDAL(connectionString));
builder.Services.AddTransient<ICategoryDAL>(catDal => new CategoryDAL(connectionString));
builder.Services.AddTransient<IClientDAL>(sp => new ClientDAL(connectionString));
builder.Services.AddTransient<IEmployeeDAL>(empDal => new EmployeeDAL(connectionString));

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
