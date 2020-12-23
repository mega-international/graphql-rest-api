using System.ComponentModel.DataAnnotations;
using Mega.Extensions.Forms;

namespace Hopex.WebService.IDE.Models
{
    public class HopexGraphQlSettings
    {
        [FormField("GraphQL schemas")]
        [Required]
        public string Schemas { get; set; } = "ITPM, Assessment, Audit, BPA, Data, DataPrivacy, ITARC, MetaModel, Reporting, Risk, Workflow";

        [FormField("Default GraphQL schema")]
        [Required]
        public string SelectedSchema { get; set; } = "ITPM";

        [FormField("Activate Voyager")]
        [Required]
        public bool IsVoyagerEnabled { get; set; } = false;
    }
}
