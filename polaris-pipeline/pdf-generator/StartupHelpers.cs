using System;
using Common.Health;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator
{
    internal static class StartupHelpers
    {
        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work.
        /// </summary>
        /// <param name="services"></param>
        internal static void BuildHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client");
        }

        internal static void SetAsposeLicence()
        {
            try
            {
                const string licenceFileName = "Aspose.Total.NET.lic";
                new Aspose.Cells.License().SetLicense(licenceFileName);
                new Aspose.Diagram.License().SetLicense(licenceFileName);
                new Aspose.Email.License().SetLicense(licenceFileName);
                new Aspose.Imaging.License().SetLicense(licenceFileName);
                new Aspose.Pdf.License().SetLicense(licenceFileName);
                new Aspose.Slides.License().SetLicense(licenceFileName);
                new Aspose.Words.License().SetLicense(licenceFileName);
            }
            catch (Exception exception)
            {
                throw new AsposeLicenseException(exception.Message);
            }
        }
    }
}
