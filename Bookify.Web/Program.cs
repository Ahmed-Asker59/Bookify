using Bookify.Web.Core.Mapping;
using Bookify.Web.Seeds;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using Bookify.Web.Helpers;
using Bookify.Web.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.DataProtection;
using WhatsAppCloudApi.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Humanizer;
using Bookify.Web.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    // User settings.
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddDataProtection().SetApplicationName(nameof(Bookify));

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
builder.Services.Configure<SecurityStampValidatorOptions>(options => 
options.ValidationInterval = TimeSpan.Zero);
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
builder.Services.AddWhatsAppApiClient(builder.Configuration);
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
builder.Services.AddExpressiveAnnotations();
builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();
builder.Services.Configure<AuthorizationOptions>(options => options.AddPolicy("AdminsOnly", policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireRole(AppRoles.Admin);
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();
var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
await DefaultRoles.SeedAsync(roleManger);
await DefaultUsers.SeedAdminUserAsync(userManager);


app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Bookify Dashboard",
    IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangFireAuthorizationFilter("AdminsOnly")
    }
});

var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
var whatsAppClient = scope.ServiceProvider.GetRequiredService<IWhatsAppClient>();
var emailBodyBuilder = scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();
var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

var hangfireTasks = new HangfireTasks(dbContext, webHostEnvironment, whatsAppClient, emailBodyBuilder, emailSender);
RecurringJob.AddOrUpdate(() => hangfireTasks.PrepeareExpirationAlert(), "0 14 * * *");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
