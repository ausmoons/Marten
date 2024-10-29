using Marten;
using MartenTaskManagment.Events;
using MartenTaskManagment.Interfaces;
using MartenTaskManagment.Models;
using MartenTaskManagment.Services;

namespace MartenTaskManagment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = "host=localhost;port=5432;database=MartenTaskManagment;username=postgres;password=postgres";

            builder.Services.AddSingleton<IDocumentStore>(provider =>
            {
                return DocumentStore.For(options =>
                {
                    options.Connection(connectionString);
                    options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;

                    options.Events.AddEventType<TaskCreated>();
                    options.Events.AddEventType<TaskAssigned>();
                    options.Events.AddEventType<TaskStatusUpdated>();

                    options.Schema.For<TaskModel>().DatabaseSchemaName("public");
                });
            });

            builder.Services.AddScoped<ITaskModelService, TaskModelService>();

            builder.Services.AddScoped(provider =>
            {
                var documentStore = provider.GetRequiredService<IDocumentStore>();
                return documentStore.OpenSession();
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
