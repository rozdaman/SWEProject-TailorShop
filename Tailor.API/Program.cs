using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Swagger ayarları için
using System.Text;
using Tailor.Business.Abstract;
using Tailor.Business.Concrete;
using Tailor.Business.Mapping;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Concrete;
using Tailor.DataAccess.Context;
using Tailor.Entity.Entities;

var builder = WebApplication.CreateBuilder(args);

// =================================================================================
// 1. VERITABANI BAGLANTISI
// =================================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =================================================================================
// 2. IDENTITY AYARLARI
// =================================================================================
builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// =================================================================================
// 3. JWT TOKEN DOGRULAMA AYARLARI
// =================================================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

// =================================================================================
// 4. AUTOMAPPER & DI (BAGIMLILIKLAR)
// =================================================================================
builder.Services.AddAutoMapper(typeof(MapProfile));

// DAL (Data Access Layer)
builder.Services.AddScoped<IProductDal, EfProductDal>();
builder.Services.AddScoped<IProductVariantDal, EfProductVariantDal>();
builder.Services.AddScoped<IOrderDal, EfOrderDal>();
builder.Services.AddScoped<IShipmentDal, EfShipmentDal>();
builder.Services.AddScoped<IShippingDal, EfShippingDal>();
builder.Services.AddScoped<IPaymentDal, EfPaymentDal>();
builder.Services.AddScoped<IAppUserDal, EfAppUserDal>();
builder.Services.AddScoped<ICategoryDal, EfCategoryDal>();
builder.Services.AddScoped<IBlogDal, EfBlogDal>();
builder.Services.AddScoped<ITestimonialDal, EfTestimonialDal>();
builder.Services.AddScoped<ISupportTicketDal, EfSupportTicketDal>();
builder.Services.AddScoped<IAddressDal, EfAddressDal>();
builder.Services.AddScoped<IContactMessageDal, EfContactMessageDal>();
builder.Services.AddScoped<IStockDal, EfStockDal>();
builder.Services.AddScoped<IStockLogDal, EfStockLogDal>();
// 1. Data Access Katmanı Bağlantısı
builder.Services.AddScoped<IAddressDal, EfAddressDal>();
builder.Services.AddScoped<IProductDisplayDal, EfProductDisplayDal>();


// SEPET MODULU
builder.Services.AddScoped<ICartDal, EfCartDal>();
builder.Services.AddScoped<ICartService, CartManager>();

// Service & Business
builder.Services.AddScoped<ILogisticsService, LogisticsManager>();
builder.Services.AddScoped<IPaymentService, PaymentManager>();
builder.Services.AddScoped<IOrderService, OrderManager>();
builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IStockService, StockManager>();
builder.Services.AddScoped<IAddressService, AddressManager>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<IFavoriteDal, EfFavoriteDal>();
builder.Services.AddScoped<IFavoriteService, FavoriteManager>();


// Generic Service
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericManager<>));

// =================================================================================
// 5. SWAGGER & CORS
// =================================================================================
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tailor API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. (Orn: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
        });
});

// =================================================================================
// APP PIPELINE
// =================================================================================
var app = builder.Build();

// --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
