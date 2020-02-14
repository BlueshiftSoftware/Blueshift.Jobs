using Blueshift.Jobs.Abstractions.Services;
using Blueshift.Jobs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.Jobs.DependencyInjection
{
    public static class BlueshiftJobsServiceCollectionExtensions
    {
        public static IServiceCollection AddBlueshiftJobs(this IServiceCollection serviceCollection)
        {
            serviceCollection = serviceCollection.AddScoped<IJobBatchService, JobBatchService>();

            return serviceCollection;
        }
    }
}
