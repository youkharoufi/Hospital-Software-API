using Hospital_Software.Services;

namespace Hospital_Software.BackgroundFunctions
{
    public class SlotRefreshBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SlotRefreshBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run the background service until the application is stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new scope to get scoped services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Get the required service
                        var slotService = scope.ServiceProvider.GetRequiredService<ISlotService>();


                        // Get all doctors and refresh their slots
                        var doctors = await slotService.GetAllDoctors();
                        foreach (var doctor in doctors)
                        {
                            await slotService.DeleteAllSlotsForDoctorAsync(doctor.Id);
                            await slotService.GenerateWeeklySlotsAsync(doctor.Id);
                        }
                    }

                    // Wait for the next day to run again
                    var nextRun = TimeSpan.FromDays(1);
                    await Task.Delay(nextRun, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken is signaled (which will throw an OperationCanceledException)
                    // since we're in a while loop and Task.Delay will throw if the application is stopped.
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                }
            }
        }
    }
}
