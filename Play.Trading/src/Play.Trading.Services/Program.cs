using System.Reflection;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Identity.Contracts;
using Play.Inventory.Contracts;
using Play.Trading.Services.Entities;
using Play.Trading.Services.Exceptions;
using Play.Trading.Services.Settings;
using Play.Trading.Services.SignalR;
using Play.Trading.Services.StateMachines;


const string AllowedOriginSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMongo()
                .AddMongoRepository<CatalogItem>("catalogitems")
                .AddMongoRepository<InventoryItem>("inventoryitems")
                .AddMongoRepository<ApplicationUser>("users")
                .AddJwtBearerAuthentication();
AddMassTransit(builder.Services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>()
                    .AddSingleton<MessageHub>()
                    .AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(xbuilder =>
    {
        xbuilder.WithOrigins(builder.Configuration[AllowedOriginSetting])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/messagehub");


void AddMassTransit(IServiceCollection services)
{
    services.AddMassTransit(configure =>
    {
        configure.UsingPlayEconomyRabbitMq(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });

        configure.AddConsumers(Assembly.GetEntryAssembly());
        configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
        {
            sagaConfigurator.UseInMemoryOutbox();
        })
                .MongoDbRepository(r =>
                {
                    var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings))
                                                                .Get<ServiceSettings>();
                    var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings))
                                                                .Get<MongoDbSettings>();


                    r.Connection = mongoSettings.ConnectionString;
                    r.DatabaseName = serviceSettings.ServiceName;
                });
    });

    var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();

    EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
    EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
    EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));

    services.AddMassTransitHostedService();
    services.AddGenericRequestClient();
}

app.Run();
