using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineStore.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using OnlineStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер
builder.Services.AddControllersWithViews();

// Добавление сервиса для работы с HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
// Добавление сервиса для работы с сессиями
builder.Services.AddSession(Options =>
{
    Options.IdleTimeout = TimeSpan.FromMinutes(10);
    Options.Cookie.HttpOnly = true;
    Options.Cookie.IsEssential = true;
});

// Конфигурация подключения к базе данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();

// Регистрация кастомного сервиса для отправки электронных писем
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Настройка конвейера обработки HTTP-запросов
if (!app.Environment.IsDevelopment())
{
    // В случае ошибки, перенаправляем на страницу ошибок
    app.UseExceptionHandler("/Home/Error");

    // Задаем политику безопасности HSTS
    // Значение HSTS по умолчанию - 30 дней. Возможно, стоит изменить это для продакшн сценариев.
    //app.UseHsts();
}

// Перенаправление с HTTP на HTTPS
//app.UseHttpsRedirection();

// Подключение статических файлов (CSS, JavaScript, изображения и т.д.)
app.UseStaticFiles();

// Настройка маршрутизации
app.UseRouting();

app.UseAuthentication();

// Проверка авторизации
app.UseAuthorization();

// Использование сессий
app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

// Запуск приложения
app.Run();