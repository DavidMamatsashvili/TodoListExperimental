using TodoListMinimalApi.Data;
using TodoListMinimalApi.Dto;
using TodoListMinimalApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<TodoListDb>(options=>options.UseNpgsql(connectionString));
builder.Services.AddDbContext<TodoListDb>(options=>options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

//if (app.Environment.IsDevelopment() || true)
//{
   // app.UseSwagger();
   // app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

//app.UseHttpsRedirection();

app.MapGet("/listitems", async(TodoListDb context) =>
{
    var res = await context.ListItems.ToListAsync();
    return Results.Ok(res);
});

app.MapGet("listitems/{id}",async(TodoListDb context, int id)=>{
    var res = await context.ListItems.FindAsync(id);
    return Results.Ok(res);    
});

app.MapPut("listitems/{id}",async(TodoListDb context, int id, ListItemDto item)=>{
    ListItem res = await context.ListItems.FindAsync(id);
    if(res==null) return Results.NotFound();
    res.Title=item.Title;
    res.Content=item.Content;
    res.Date=item.Date;
    res.Author=item.Author;
    await context.SaveChangesAsync();
    return Results.NoContent();   
});

app.MapPost("listitems/newitem",async(TodoListDb context, ListItemDto item)=>{
    if(item==null) return Results.NotFound();
    context.Add(item);
    await context.SaveChangesAsync();
    return Results.Created("listitems/{item.Id}",item);    
});

app.MapDelete("listitems/{id}",async(TodoListDb context, int id)=>{
    var res = await context.ListItems.FindAsync(id);
    if(res==null) return Results.NotFound();
    context.ListItems.Remove(res);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

