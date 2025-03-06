using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Service;
using suivi_abonnement.Repository;
using suivi_abonnement.Hubs;
using Rotativa.AspNetCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services nécessaires
builder.Services.AddDistributedMemoryCache();  // Utilisation de la mémoire pour stocker les données de session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Durée de la session
    options.Cookie.HttpOnly = true;  // La session ne sera accessible que côté serveur
    options.Cookie.IsEssential = true;  // Cookie essentiel pour la session
});

// Ajouter les services HTTP Context Accessor pour gérer les sessions
builder.Services.AddHttpContextAccessor();

// Ajouter les services Razor et MVC
builder.Services.AddRazorPages();  // ou AddServerSideBlazor() si vous utilisez Blazor Server
builder.Services.AddControllersWithViews();  // Pour les contrôleurs avec vues
builder.Services.AddSignalR(); // 👈 Enregistre SignalR
builder.Services.AddHttpContextAccessor();

// Ajouter les services de configuration de la base de données
builder.Services.AddScoped<IAbonnementService, AbonnementService>();
builder.Services.AddScoped<IAbonnementRepository, AbonnementRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartementService, DepartementService>();
builder.Services.AddScoped<IDepartementRepository, DepartementRepository>();
builder.Services.AddScoped<IFournisseurService, FournisseurService>();
builder.Services.AddScoped<IFournisseurRepository, FournisseurRepository>();
builder.Services.AddScoped<ICategorieService, CategorieService>();
builder.Services.AddScoped<ICategorieRepository, CategorieRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotifyEmailRepository, NotifyEmailRepository>();
builder.Services.AddScoped<INotifyEmailService, NotifyEmailService>();
builder.Services.AddScoped<IDirectionRepository, DirectionRepository>();
builder.Services.AddScoped<IDirectionService, DirectionService>();


// Configurer les services nécessaires

var app = builder.Build();


// Configurer le pipeline de requêtes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//Package Rotativa.AspNetCore pour les fichiers PDF
app.UseRotativa();

app.UseRouting();

// Ajouter le middleware de session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthClient}/{action=Login}/{id?}");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});


app.MapHub<MessageHub>("/messageHub");  // 👈 Ajoute le hub SignalR
app.MapHub<NotificationHub>("/notificationHub");  // 👈 Ajoute le hub SignalR
app.Run();
