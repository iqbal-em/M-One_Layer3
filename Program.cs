using Device.Core.Interfaces;
using Device.Core.SDK.Bioslim10;
using Device.Core.Services;
using M_One_Layer3;
using M_One_Layer3.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using M_One_Layer3.Infrastructure.Database;
//using M_One_Layer3.Enrollment_db;

var builder = WebApplication.CreateBuilder(args);

//Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
// =========================
// 🔹 REGISTER SERVICES
// =========================
builder.Services.AddSingleton<IFingerprintWrapper, BioSlim10SDKWrapper>();
builder.Services.AddSingleton<IFingerPrintService, FingerprintService>();
builder.Services.AddSingleton<FingerprintService_Bridge>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=demo_device.db"));

builder.Services.AddTransient<ThermalPrinterService58mm>(sp =>
    new ThermalPrinterService58mm("COM8", 9600));
//builder.Services.AddScoped<Enrollment_Fingerprint>();

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =========================
// 🔥 PAKSA BRIDGE DIBUAT SAAT STARTUP
// =========================
app.Services.GetRequiredService<FingerprintService_Bridge>();

// =========================
// 🔹 MIDDLEWARE PIPELINE
// =========================
app.UseStaticFiles();
app.UseRouting();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Registration}/{action=Index}/{id?}");

/*app.MapGet("/runtime", () =>
{
    return new
    {
        OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
        Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
    };
});*/

app.MapControllers();
app.MapHub<FingerprintHub>("/fingerprinthub");

app.Run();
