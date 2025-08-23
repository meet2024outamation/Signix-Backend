using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Signix.IAM.Entities;

namespace Signix.IAM.API.Endpoints.Client
{
    public class ClientCreateRequest
    {
        [StringLength(250)]
        [Unicode(false)]
        public string ClientName { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string City { get; set; }

        [StringLength(2)]
        [Unicode(false)]
        public string State { get; set; }

        public string ZipCode { get; set; }

        public string ClientAddress { get; set; }

        public string Phone { get; set; }

        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        //public int? ClientTypeId { get; set; }

        //public string? Note { get; set; }

        //[StringLength(50)]
        //[Unicode(false)]
        //public string? PhysicalLineText { get; set; }

        //[StringLength(50)]
        //[Unicode(false)]
        //public string? PhysicalAdditionalLineText { get; set; }

        //[StringLength(13)]
        //[Unicode(false)]
        //public string? PhysicalPostalCode { get; set; }

        //[StringLength(50)]
        //[Unicode(false)]
        //public string? MailingLineText { get; set; }

        //[StringLength(50)]
        //[Unicode(false)]
        //public string? MailingAdditionalLineText { get; set; }

        //[StringLength(50)]
        //[Unicode(false)]
        //public string? MailingCityName { get; set; }

        //[StringLength(2)]
        //[Unicode(false)]
        //public string? MailingStateCode { get; set; }

        //[StringLength(13)]
        //[Unicode(false)]
        //public string? MailingPostalCode { get; set; }

        //public int? VendorTypeId { get; set; }

        //[StringLength(100)]
        //[Unicode(false)]
        //public string? VendorName { get; set; }

        //[StringLength(100)]
        //[Unicode(false)]
        //public string? VendorTaxpayerIdentifierValue { get; set; }

        //[StringLength(100)]
        //[Unicode(false)]
        //public string InitialAdminFirstName { get; set; } = null!;

        //[StringLength(100)]
        //[Unicode(false)]
        //public string? InitialAdminMiddleName { get; set; }

        //[StringLength(100)]
        //[Unicode(false)]
        //public string InitialAdminLastname { get; set; } = null!;

        //[StringLength(100)]
        //[Unicode(false)]
        //public string InitialAdminEmail { get; set; } = null!;

        //[StringLength(20)]
        //[Unicode(false)]
        //public string DatabaseCreationSource { get; set; } = null!;

    }
}
