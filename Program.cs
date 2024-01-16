using System.Collections.Generic;
using Hospital_Software.BackgroundFunctions;
using Hospital_Software.ChatServices;
using Hospital_Software.Data;
using Hospital_Software.Data.MongoDbStores;
using Hospital_Software.Data.Seeds;
using Hospital_Software.Models;
using Hospital_Software.Services;
using Hospital_Software.Token;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddUserStore<MyMongoUserStore>()
    .AddRoleStore<MyMongoDbRoleStore>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<Hospital_Software.Data.MongoDbContext>();

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var configuration = serviceProvider.GetService<IConfiguration>();
    var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
    return client.GetDatabase(configuration["MongoDB:DatabaseName"]);
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IRoleStore<ApplicationRole>, MyMongoDbRoleStore>();

builder.Services.AddHostedService<SlotRefreshBackgroundService>();

builder.Services.AddScoped<ISlotService, SlotService>();

builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddCors();

builder.Services.AddSignalR();


// Swagger configuration...
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

app.UseStaticFiles();

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200"));

app.UseRouting();

app.UseAuthentication(); // Added for Identity
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var mongoDbContext = services.GetRequiredService<Hospital_Software.Data.MongoDbContext>();

    var slotCollection = mongoDbContext.Slots;
    var userCollection = mongoDbContext.Users;

    // Seeding Users and Roles
    await DbSeeder.SeedUsersAsync(userManager, mongoDbContext);
    await DbSeeder.SeedRolesAsync(roleManager);

    // Retrieve all users with the "Doctor" role
    var filter = Builders<ApplicationUser>.Filter.Where(u => u.RoleName == "Doctor");
    var doctorUsers = await userCollection.Find(filter).ToListAsync();

    var existingSlotsCount = await slotCollection.CountDocumentsAsync(FilterDefinition<Slot>.Empty);

    if (existingSlotsCount == 0)
    {
        foreach (var doctor in doctorUsers)
        {
            await DbSeeder.SeedSlotsForDoctorAsync(slotCollection, doctor.Id); // Assuming doctor.Id is the identifier
        }
    }
    // Seed Slots for each doctor
    
}





app.Run();
