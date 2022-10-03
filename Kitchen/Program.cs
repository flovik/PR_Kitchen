using Kitchen.Interfaces;
using Kitchen.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IKitchenSender, KitchenSender>();
builder.Services.AddSingleton<IKitchenService, KitchenService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
