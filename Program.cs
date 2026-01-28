using BurgerJoint.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BurgerJoint.Models;

var builder = WebApplication.CreateBuilder(args);

// =====  SERVICES  =====
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//  AUTH  (must be BEFORE Build())
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// =====  PIPELINE  =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   //  add these two
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Burger}/{action=Index}/{id?}");

//  SEED  (optional)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Burgers.Any())
    {
        db.Burgers.AddRange(
            new Burger { Name = "Hamburger", Description = "Classic beef patty with lettuce, tomato and onion", Price = 45.00m, Ingredients = "Beef, Lettuce, Tomato, Onion, Bun" },
            new Burger { Name = "Chicken Burger", Description = "Grilled chicken breast with mayo and lettuce", Price = 50.00m, Ingredients = "Chicken, Mayo, Lettuce, Bun" },
            new Burger { Name = "Double Cheeseburger", Description = "Two beef patties loaded with cheese", Price = 65.00m, Ingredients = "Beef, Cheese, Pickle, Bun" },
            new Burger { Name = "Dagwood Burger", Description = "Triple-layer monster with egg and bacon", Price = 85.00m, Ingredients = "Beef, Egg, Bacon, Cheese, Bun" },
            new Burger { Name = "BBQ Ranch Burger", Description = "Beef, crispy onions, ranch, BBQ sauce", Price = 70.00m, Ingredients = "Beef, Onion Rings, Ranch, BBQ, Bun" },
            new Burger { Name = "Peri-Peri Chicken", Description = "Spicy grilled chicken, peri mayo, coleslaw", Price = 55.00m, Ingredients = "Chicken, Peri Mayo, Slaw, Bun" },
            new Burger { Name = "Mushroom Swiss", Description = "Beef, sautéed mushrooms, Swiss cheese", Price = 68.00m, Ingredients = "Beef, Mushroom, Swiss, Bun" },
            new Burger { Name = "Jalapeño Popper", Description = "Beef, cream-cheese-stuffed jalapeños, cheddar", Price = 72.00m, Ingredients = "Beef, Jalapeño, Cream Cheese, Cheddar, Bun" },
            new Burger { Name = "Pineapple Teriyaki", Description = "Beef, grilled pineapple, teriyaki glaze", Price = 66.00m, Ingredients = "Beef, Pineapple, Teriyaki, Bun" },
            new Burger { Name = "Vegan Bliss", Description = "Plant-based patty, vegan cheese, avo", Price = 60.00m, Ingredients = "Plant Patty, Vegan Cheese, Avo, Bun" },
            new Burger { Name = "Breakfast Burger", Description = "Beef, egg, bacon, hash brown, maple glaze", Price = 78.00m, Ingredients = "Beef, Egg, Bacon, Hash Brown, Maple, Bun" },
            new Burger { Name = "Truffle Shuffle", Description = "Beef, truffle mayo, parmesan shards", Price = 90.00m, Ingredients = "Beef, Truffle Mayo, Parmesan, Bun" }
        );
        db.SaveChanges();
    }
}

app.Run();