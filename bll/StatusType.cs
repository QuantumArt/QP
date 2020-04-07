using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class StatusType : EntityObject
    {
        private const int MaxWeight = 100;

        [Display(Name = "Weight", ResourceType = typeof(StatusTypeStrings))]
        public int Weight { get; set; }

        [Display(Name = "Color", ResourceType = typeof(StatusTypeStrings))]
        [StringLength(6, ErrorMessageResourceName = "ColorLengthExceeded", ErrorMessageResourceType = typeof(StatusTypeStrings))]
        [RegularExpression(RegularExpressions.RgbColor, ErrorMessageResourceName = "ColorInvalidFormat", ErrorMessageResourceType = typeof(StatusTypeStrings))]
        public string Color { get; set; }

        [StringLength(6, ErrorMessageResourceName = "AltColorLengthExceeded", ErrorMessageResourceType = typeof(StatusTypeStrings))]
        [RegularExpression(RegularExpressions.RgbColor, ErrorMessageResourceName = "AltColorInvalidFormat", ErrorMessageResourceType = typeof(StatusTypeStrings))]
        [Display(Name = "AltColor", ResourceType = typeof(StatusTypeStrings))]
        public string AltColor { get; set; }

        public int SiteId { get; set; }

        public bool BuiltIn { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.StatusType;

        public override int ParentEntityId => SiteId;

        public static StatusType GetPublished(int siteId) => StatusTypeRepository.GetByName(StatusName.Published, siteId);

        public static StatusType GetNone(int siteId) => StatusTypeRepository.GetByName(StatusName.None, siteId);

        public static StatusType Create(int siteId) => new StatusType { SiteId = siteId, Weight = MinFreeWeight(StatusTypeRepository.GetWeightsBySiteId(siteId)) };

        private static int MinFreeWeight(IEnumerable<int> occupiedWeights) => Enumerable.Range(0, MaxWeight + 1).Except(occupiedWeights).Min();

        public override void Validate()
        {
            var errors = new RulesException<StatusType>();
            if (string.IsNullOrEmpty(Color) && !string.IsNullOrEmpty(AltColor))
            {
                errors.ErrorFor(n => n.Color, StatusTypeStrings.ColorMissed);
            }

            if (string.IsNullOrEmpty(AltColor) && !string.IsNullOrEmpty(Color))
            {
                errors.ErrorFor(n => n.AltColor, StatusTypeStrings.AltColorMissed);
            }

            ValidateWeight(errors);
            base.Validate(errors);
            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private void ValidateWeight(RulesException<StatusType> errors)
        {
            if (StatusTypeRepository.GetWeightsBySiteId(SiteId, Id).Contains(Weight))
            {
                errors.ErrorFor(n => n.Weight, StatusTypeStrings.WeightIsInUse);
            }
        }
    }
}
