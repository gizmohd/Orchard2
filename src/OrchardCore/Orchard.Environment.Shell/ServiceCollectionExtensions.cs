using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.Descriptor.Settings;

namespace Orchard.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddScoped<IShellDescriptorManager, AllFeaturesShellDescriptorManager>();

            return services;
        }

        public static IServiceCollection AddSetFeaturesDescriptor(this IServiceCollection services, IEnumerable<ShellFeature> shellFeatures)
        {
            services.AddSingleton<IShellDescriptorManager>(new SetFeaturesShellDescriptorManager(shellFeatures));

            return services;
        }
    }
}