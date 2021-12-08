using System;
using ExternalPortal.Constants;
using Microsoft.AspNetCore.Http;
using OfgemAPIHelpers = Ofgem.API.GGSS.Application.Helpers;

namespace ExternalPortal.Models
{
    public class EmailParameterBuilder
    {
        private readonly EmailParameter _emailParameter;

        public EmailParameterBuilder(string templateId, string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new ArgumentNullException("templateId");
            }

            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentNullException("emailAddress");
            }

            _emailParameter = new EmailParameter(templateId, emailAddress);
        }

        public EmailParameterBuilder AddFirstName(string firstName)
        {
            _emailParameter.AddPersonalisation("FirstName", firstName);

            return this;
        }

        public EmailParameterBuilder AddLastName(string lastName)
        {
            _emailParameter.AddPersonalisation("LastName", lastName);

            return this;
        }

        public EmailParameterBuilder AddFullName(string fullName)
        {
            _emailParameter.AddPersonalisation("FullName", fullName);

            return this;
        }

        public EmailParameterBuilder AddApplicationId(Guid applicationId)
        {
            _emailParameter.AddPersonalisation("ApplicationId", OfgemAPIHelpers.ReferenceNumber.GetApplicationReference(applicationId, null));

            return this;
        }

        public EmailParameterBuilder AddDashboardLink(string scheme, HostString host, Guid applicationId)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = scheme,
                Host = host.Host.ToString(),
                Path = $"{UrlKeys.TaskList}/{applicationId}"
            };

            _emailParameter.AddPersonalisation("DashboardLink", uriBuilder.Uri.ToString());

            return this;
        }

        public EmailParameterBuilder AddCustom(string key, dynamic value)
        {
            _emailParameter.AddPersonalisation(key, value);

            return this;
        }

        public EmailParameterBuilder AddDate(string key, DateTime date)
        {
            _emailParameter.AddPersonalisation(key, date.ToString("dd MMMM yyyy"));

            return this;
        }

        public EmailParameter Build()
        {
            return _emailParameter;
        }
    }
}
