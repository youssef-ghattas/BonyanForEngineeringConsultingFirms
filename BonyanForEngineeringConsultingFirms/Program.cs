namespace BonyanForEngineeringConsultingFirms;
using Bonyan.BLL.Services;
using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Bonyan.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Task = Bonyan.DAL.Models.Task;
using BonyanForEngineeringConsultingFirms.Services;

public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

		// ?? Register DbContext ????????????????????????????????
		builder.Services.AddDbContext<BonyanDbContext>(options =>
			options.UseSqlServer(builder.Configuration
			.GetConnectionString("BonyanConnection")));

        // ── Session ───────────────────────────────────────────
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // ?? Repositories ??????????????????????????????????????
        builder.Services.AddScoped<IRepository<Employee>, Repository<Employee>>();
		builder.Services.AddScoped<IRepository<UserAccount>, Repository<UserAccount>>();
		builder.Services.AddScoped<IRepository<Project>, Repository<Project>>();
		builder.Services.AddScoped<IRepository<EmployeeProject>, Repository<EmployeeProject>>();
		builder.Services.AddScoped<IRepository<Task>, Repository<Task>>();
		builder.Services.AddScoped<IRepository<Document>, Repository<Document>>();
		builder.Services.AddScoped<IRepository<Drawing>, Repository<Drawing>>();
		builder.Services.AddScoped<IRepository<SiteVisit>, Repository<SiteVisit>>();
		builder.Services.AddScoped<IRepository<Invoice>, Repository<Invoice>>();
		builder.Services.AddScoped<IRepository<Payment>, Repository<Payment>>();
		builder.Services.AddScoped<IRepository<Material>, Repository<Material>>();
		builder.Services.AddScoped<IRepository<MaterialInventory>, Repository<MaterialInventory>>();
		builder.Services.AddScoped<IRepository<MaterialSupplier>, Repository<MaterialSupplier>>();
		builder.Services.AddScoped<IRepository<MaterialTask>, Repository<MaterialTask>>();
		builder.Services.AddScoped<IRepository<Supplier>, Repository<Supplier>>();
		builder.Services.AddScoped<IRepository<Inventory>, Repository<Inventory>>();
		builder.Services.AddScoped<IRepository<Admin>, Repository<Admin>>();
		builder.Services.AddScoped<IRepository<AdminAccount>, Repository<AdminAccount>>();

		// ?? Services ??????????????????????????????????????????
		builder.Services.AddScoped<IService<Employee>, Service<Employee>>();
		builder.Services.AddScoped<IService<UserAccount>, Service<UserAccount>>();
		builder.Services.AddScoped<IService<Project>, Service<Project>>();
		builder.Services.AddScoped<IService<EmployeeProject>, Service<EmployeeProject>>();
		builder.Services.AddScoped<IService<Task>, Service<Task>>();
		builder.Services.AddScoped<IService<Document>, Service<Document>>();
		builder.Services.AddScoped<IService<Drawing>, Service<Drawing>>();
		builder.Services.AddScoped<IService<SiteVisit>, Service<SiteVisit>>();
		builder.Services.AddScoped<IService<Invoice>, Service<Invoice>>();
		builder.Services.AddScoped<IService<Payment>, Service<Payment>>();
		builder.Services.AddScoped<IService<Material>, Service<Material>>();
		builder.Services.AddScoped<IService<MaterialInventory>, Service<MaterialInventory>>();
		builder.Services.AddScoped<IService<MaterialSupplier>, Service<MaterialSupplier>>();
		builder.Services.AddScoped<IService<MaterialTask>, Service<MaterialTask>>();
		builder.Services.AddScoped<IService<Supplier>, Service<Supplier>>();
		builder.Services.AddScoped<IService<Inventory>, Service<Inventory>>();
		builder.Services.AddScoped<IService<Admin>, Service<Admin>>();
		builder.Services.AddScoped<IService<AdminAccount>, Service<AdminAccount>>();

		builder.Services.AddScoped<BonyanForEngineeringConsultingFirms.Services.EmailService>();


        // Chatbot support services — ChatDataService fetches role-filtered
        // DB data, GroqService sends it to the LLM API.
        builder.Services.AddScoped<ChatDataService>();
        builder.Services.AddScoped<GroqService>();
        builder.Services.AddHttpClient<GroqService>();

		// Add services to the container.
		builder.Services.AddControllersWithViews();

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
			app.UseSession();
			app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Landing}/{id?}");

        app.Run();
        }
    }
