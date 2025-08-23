using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Signix.IAM.Entities;

namespace Signix.IAM.API.Endpoints.Client
{
    public class ClientGetResponse
    {

        public string Id { get; set; }
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

        public DateTimeOffset CreatedDate { get; set; }

        public string CreatedBy { get; set; }

    }
}
