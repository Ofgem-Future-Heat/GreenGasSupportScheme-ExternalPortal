using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Ofgem.API.GGSS.Domain.Models;

namespace ExternalPortal.Services
{
    public interface IGetCompaniesHouseService
    {
        Task<GetCompaniesHouseResponse> GetCompanyDetailsAsync(string registrationNumber, CancellationToken token = default);
    }

    public class FakeGetCompaniesHouseService : IGetCompaniesHouseService
    {
        public FakeGetCompaniesHouseService() { }

        public Task<GetCompaniesHouseResponse> GetCompanyDetailsAsync(string registrationNumber, CancellationToken token = default)
        {
            CheckParameter(registrationNumber);

            var companiesHouseResponse = new GetCompaniesHouseResponse
            {
                Model = new OrganisationModel
                {
                    Value = new Ofgem.API.GGSS.Domain.ModelValues.OrganisationValue
                    {
                        Name = "Made Tech",
                        RegistrationNumber = "12345678",
                        RegisteredOfficeAddress = new AddressModel
                        {
                            LineOne = "4 O'Meara St",
                            LineTwo = "",
                            Town = "London",
                            County = "Greater London",
                            Postcode = "SE1 1TE"
                        }
                    }
                }
            };

            return Task.FromResult(companiesHouseResponse);
        }

        private void CheckParameter(string registrationNumber)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
            {
                throw new System.ArgumentNullException(nameof(registrationNumber));
            }
        }
    }

    public class GetCompaniesHouseService : IGetCompaniesHouseService
    {
        private readonly HttpClient _httpClient;

        public GetCompaniesHouseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GetCompaniesHouseResponse> GetCompanyDetailsAsync(string registrationNumber, CancellationToken token = default)
        {
            CheckParameter(registrationNumber);

            var companiesHouseResponse = new GetCompaniesHouseResponse();

            try
            {
                var response = await _httpClient.GetAsync($"/home/{registrationNumber}", token);

                if (!response.IsSuccessStatusCode)
                {
                    companiesHouseResponse.AddError($"Companies house service failed to return successfully - reason: {response.StatusCode}");

                    return companiesHouseResponse;
                }

                var result = JsonSerializer.Deserialize<OrganisationModelResponse>(await response.Content.ReadAsStringAsync());

                companiesHouseResponse.Model = new OrganisationModel
                {
                    Value = new Ofgem.API.GGSS.Domain.ModelValues.OrganisationValue
                    {
                        Name = result.Name,
                        RegistrationNumber = result.RegistrationNumber,
                        RegisteredOfficeAddress = new AddressModel
                        {
                            LineOne = result.AddressModel.LineOne,
                            LineTwo = result.AddressModel.LineTwo,
                            Town = result.AddressModel.Town,
                            County = result.AddressModel.County,
                            Postcode = result.AddressModel.PostCode
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                companiesHouseResponse.AddError($"Companies house service failed to return successfully - reason: {ex.Message}");
            }

            return companiesHouseResponse;
        }

        private void CheckParameter(string registrationNumber)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
            {
                throw new System.ArgumentNullException(nameof(registrationNumber));
            }
        }
    }

    public class GetCompaniesHouseResponse
    {
        private readonly List<string> _errors = new List<string>();

        public ReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        public OrganisationModel Model { get; set; }

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }

    /// <summary>
    /// Remove internal class when OrganisationModel updated with JsonPropertyName attributes
    /// </summary>
    internal class OrganisationModelResponse
    {
        [JsonPropertyName("company_name")]
        public string Name { get; set; }

        [JsonPropertyName("company_number")]
        public string RegistrationNumber { get; set; }

        [JsonPropertyName("registered_office_address")]
        public AddressModelResponse AddressModel { get; set; }
    }

    /// <summary>
    /// Remove internal class when AddressModel updated with JsonPropertyName attributes
    /// </summary>
    internal class AddressModelResponse
    {
        [JsonPropertyName("address_line_1")]
        public string LineOne { get; set; }

        [JsonPropertyName("address_line_2")]
        public string LineTwo { get; set; }

        [JsonPropertyName("locality")]
        public string Town { get; set; }

        [JsonPropertyName("region")]
        public string County { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostCode { get; set; }
    }


}
