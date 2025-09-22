using System.ComponentModel.DataAnnotations;

namespace Espadium.Wiki.Api.Configuration.Options
{
    public class ContentLimitsOptions
    {
        [Range(1, int.MaxValue)]
        public int MaxPageLengthChars { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxTitleLength { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxRevisionsPerPage { get; set; }
    }
}

