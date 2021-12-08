using ExternalPortal.Extensions;
using ExternalPortal.ViewModels;
using Ofgem.API.GGSS.Domain.Models;
using System.Linq;
using Xunit;

namespace ExternalPortal.UnitTests.Models
{
    public class CompaniesHouseTests
    {
        [Fact]
        public void ShouldReturnErrorMessageWhenSet()
        {
            const string errorMessage = "This is an error message";

            var model = new PortalViewModel<OrganisationModel>();
            model.Errors.Add(errorMessage);

            Assert.True(model.HasErrors);
            Assert.Equal(errorMessage, model.Errors.FirstOrDefault());
        }

        [Fact]
        public void ShouldReturnNotIsErrorWhenErrorMessageIsNotSet()
        {
            var vm = new PortalViewModel<OrganisationModel> { Model = new OrganisationModel { Value = new Ofgem.API.GGSS.Domain.ModelValues.OrganisationValue { RegistrationNumber = "fake-number" } } };

            Assert.False(vm.HasErrors);
            Assert.Null(vm.Errors.FirstOrDefault());
        }

        [Fact]
        public void ShouldReturnCompaniesHouseAddressAsHtml()
        {
            const string address = "address-line-1<br />address-line-2<br />XX1 1XX<br />country";

            var model = new PortalViewModel<OrganisationModel>()
            {
                Model = new OrganisationModel()
                {
                    Value = new Ofgem.API.GGSS.Domain.ModelValues.OrganisationValue
                    {
                        RegisteredOfficeAddress = new AddressModel
                        {
                            LineOne = "address-line-1",
                            LineTwo = "address-line-2",
                            Postcode = "XX1 1XX",
                            County = "country"
                        }
                    }
                }
            };

            Assert.Equal(address, model.Model.Value.RegisteredOfficeAddress.ToHtmlString());
        }

        [Fact]
        public void ShouldReturnCompaniesHouseAddressWithoutAddressLine2AsHtml()
        {
            const string address = "address-line-1<br />XX1 1XX<br />country";

            var model = new PortalViewModel<OrganisationModel>()
            {
                Model = new OrganisationModel()
                {
                    Value = new Ofgem.API.GGSS.Domain.ModelValues.OrganisationValue
                    {
                        RegisteredOfficeAddress = new AddressModel
                        {
                            LineOne = "address-line-1",
                            Postcode = "XX1 1XX",
                            County = "country"
                        }
                    }
                }
            };

            Assert.Equal(address, model.Model.Value.RegisteredOfficeAddress.ToHtmlString());
        }
    }
}
