﻿using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Siccar.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using CommonConstants = Siccar.Common.Constants;

namespace WalletService.Configuration
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently
            foreach (var description in provider.ApiVersionDescriptions)
            {
          //      options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "Wallet Service",
                Version = "v1",
                Description = "Creates wallets and builds transactions, part of Siccar.",
                TermsOfService = new Uri(CommonConstants.TermsOfServiceURI),
                Contact = new OpenApiContact
                {
                    Name = CommonConstants.ContactName,
                    Email = CommonConstants.ContactEmail,
                    Url = new Uri("https://www.siccar.net/"),
                },
                License = new OpenApiLicense
                {
                    Name = CommonConstants.LicenseName,
                    Url = new Uri(CommonConstants.LicenseURI)
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
