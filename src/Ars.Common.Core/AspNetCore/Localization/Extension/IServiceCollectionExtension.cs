﻿using Ars.Commom.Core;
using Ars.Common.Core.Configs;
using Ars.Common.Core.Localization.Options;
using Ars.Common.Core.Localization.ValidProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;

namespace Ars.Common.Core.Localization.Extension
{
    public static class IServiceCollectionExtension
    {
        public static IArsWebApplicationBuilder AddArsLocalization(this IArsWebApplicationBuilder arsServiceProvider)
        {
           var arsLocalizationOption = arsServiceProvider.Configuration
                .GetSection(nameof(ArsLocalizationConfiguration))
                .Get<ArsLocalizationConfiguration>() 
                ?? new ArsLocalizationConfiguration() { Cultures = new[] { "en-US", "zh-Hans" } };
            var arsconfig = arsServiceProvider.ServiceProvider.GetRequiredService<IArsConfiguration>();
            arsconfig.ArsLocalizationConfiguration ??= arsLocalizationOption;
            arsServiceProvider.Services.AddSingleton<IArsLocalizationConfiguration>(arsLocalizationOption);
            arsconfig.AddArsAppExtension(new ArsLocalizationAppExtension());

            var services = arsServiceProvider.Services;
            services.AddLocalization(
                option =>
                     option.ResourcesPath = arsLocalizationOption.ResourcesPath);

            IMvcBuilder builder;
            if (arsLocalizationOption.IsAddViewLocalization)
                builder = services.AddControllersWithViews();
            else
                builder = services.AddControllers();

            builder.AddArsViewLocalization(arsLocalizationOption);

            services.Configure<RequestLocalizationOptions>(option =>
            {
                CultureInfo[] cultureInfos = 
                    arsLocalizationOption.Cultures
                        .Distinct()
                        .Select(r => new CultureInfo(r)).ToArray();

                option.DefaultRequestCulture = new RequestCulture(arsLocalizationOption.DefaultRequestCulture);
                // Formatting numbers, dates, etc.
                option.SupportedCultures = cultureInfos;
                // UI strings that we have localized.
                option.SupportedUICultures = cultureInfos;
                //The Content-Language header can be added by setting the property ApplyCurrentCultureToResponseHeaders.
                option.ApplyCurrentCultureToResponseHeaders = true;
            });

            services.AddSingleton<IArstringLocalizer, ArstringLocalizer>();
            return arsServiceProvider;
        }

        private static IMvcBuilder AddArsViewLocalization(this IMvcBuilder mvcBuilder, ArsLocalizationConfiguration arsLocalizationOption)
        {
            mvcBuilder.AddMvcOptions(option => option.ModelMetadataDetailsProviders.Add(new ValidationMetadataLocalizationProvider()));
            if (arsLocalizationOption.IsAddViewLocalization)
                mvcBuilder.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);//AddViewLocalization adds support for localized view files.
                                                                                          //In this sample view localization is based on the view file suffix. For example "fr" in the Index.fr.cshtml file.
            if (arsLocalizationOption.IsAddDataAnnotationsLocalization)
                mvcBuilder.AddDataAnnotationsLocalization(options =>  //AddDataAnnotationsLocalization adds support for localized DataAnnotations validation messages through IStringLocalizer abstractions.
                {
                    options.DataAnnotationLocalizerProvider = 
                         (_, factory) => factory.Create(nameof(ArshareResource),new AssemblyName(Assembly.GetEntryAssembly()!.FullName!).Name!);
                });

            return mvcBuilder;
        }
    }
}
