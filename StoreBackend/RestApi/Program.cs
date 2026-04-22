using RestApi.Setup;
using RestApi.Users.Authorization;

var builder = WebApplication.CreateBuilder(args);



builder.AddData();
builder.AddProductsModule();
builder.AddUsersModule();
builder.AddAuth();



builder.Services.AddAuthorization();


builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StoreApiCorsPolicy",policy =>
    {
        policy.WithOrigins("http://localhost:5127", "https://localhost:7130")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("StoreApiCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapOpenApi();

app.EnableAutoMigration();



app.Run();