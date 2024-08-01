using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace Aspire.AppHost.Extensions;

internal static class DistributedApplicationBuilderExtensions
{
    public static IDistributedApplicationBuilder AddForwardedHeaders(
        this IDistributedApplicationBuilder builder
    )
    {
        builder.Services.TryAddLifecycleHook<AddForwardHeadersHook>();
        return builder;
    }

    private class AddForwardHeadersHook : IDistributedApplicationLifecycleHook
    {
        public Task BeforeStartAsync(
            DistributedApplicationModel appModel,
            CancellationToken cancellationToken = default
        )
        {
            foreach (var p in appModel.GetProjectResources())
            {
                p.Annotations.Add(
                    new EnvironmentCallbackAnnotation(context =>
                    {
                        context.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] =
                            "true";
                    })
                );
            }

            return Task.CompletedTask;
        }
    }
}
