using Hospital_Software.Data;
using Hospital_Software.Data.MongoDbStores;
using Hospital_Software.Data.Seeds;
using Hospital_Software.Models;
using Hospital_Software.Token;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

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

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var configuration = serviceProvider.GetService<IConfiguration>();
    var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
    return client.GetDatabase(configuration["MongoDB:DatabaseName"]);
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IRoleStore<ApplicationRole>, MyMongoDbRoleStore>();


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

app.UseAuthentication(); // Added for Identity
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var mongoDbContext = services.GetRequiredService<MongoDbContext>();

    var userCollection = mongoDbContext.MyMongoDocuments; // This is the correct way to initialize userCollection


    // Seeding Users and Roles
    await DbSeeder.SeedUsersAsync(userManager, mongoDbContext);
    await DbSeeder.SeedRolesAsync(roleManager);

    // Retrieve all users with the "Doctor" role
    var filter = Builders<ApplicationUser>.Filter.Where(u => u.RoleName == "Doctor");
    var doctorUsers = await userCollection.Find(filter).ToListAsync();
    // Seed Slots for each doctor
    foreach (var doctor in doctorUsers)
    {
        await DbSeeder.SeedSlotsAsync(mongoDbContext.Slots, doctor.Id);
    }
}




app.Run();
