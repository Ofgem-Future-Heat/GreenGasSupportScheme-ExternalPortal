using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ExternalPortal.Models
{
    public class EmailParameter
    {
        public string TemplateId { get; }

        public string EmailAddress { get; }

        public string Reference => Guid.NewGuid().ToString();

        public FormUrlEncodedContent Content => GetFormContent();

        private readonly List<KeyValuePair<string, string>> _keyValuePairs;

        public EmailParameter(string templateId, string emailAddress)
        {
            TemplateId = templateId;
            EmailAddress = emailAddress;

            _keyValuePairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(TemplateId), TemplateId),
                new KeyValuePair<string, string>(nameof(EmailAddress), EmailAddress),
                new KeyValuePair<string, string>(nameof(Reference), Reference)
            };
        }

        public void AddPersonalisation(string key, string value)
        {
            var newKeyValue = new KeyValuePair<string, string>(key, value);

            if (_keyValuePairs.Exists(k => k.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ArgumentException($"Key '{key}' already in key value pair");
            }

            _keyValuePairs.Add(newKeyValue);
        }

        private FormUrlEncodedContent GetFormContent()
        {
            return new FormUrlEncodedContent(_keyValuePairs);
        }
    }
}
