using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class StatusType : EntityObject
    {
        private const int MaxWeight = 100;

        [LocalizedDisplayName("Weight", NameResourceType = typeof(StatusTypeStrings))]
        public int Weight { get; set; }

        [LocalizedDisplayName("Color", NameResourceType = typeof(StatusTypeStrings))]
        [MaxLengthValidator(6, MessageTemplateResourceName = "ColorLengthExceeded", MessageTemplateResourceType = typeof(StatusTypeStrings))]
        [FormatValidator(RegularExpressions.RgbColor, MessageTemplateResourceName = "ColorInvalidFormat", MessageTemplateResourceType = typeof(StatusTypeStrings))]
        public string Color { get; set; }

        [MaxLengthValidator(6, MessageTemplateResourceName = "AltColorLengthExceeded", MessageTemplateResourceType = typeof(StatusTypeStrings))]
        [FormatValidator(RegularExpressions.RgbColor, MessageTemplateResourceName = "AltColorInvalidFormat", MessageTemplateResourceType = typeof(StatusTypeStrings))]
        [LocalizedDisplayName("AltColor", NameResourceType = typeof(StatusTypeStrings))]
        public string AltColor { get; set; }

        public int SiteId { get; set; }

        public bool BuiltIn { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.StatusType;

        public override int ParentEntityId => SiteId;

        public static StatusType GetPublished(int siteId)
        {
            return StatusTypeRepository.GetByName(StatusName.Published, siteId);
        }

        public static StatusType GetNone(int siteId)
        {
            return StatusTypeRepository.GetByName(StatusName.None, siteId);
        }

        public static StatusType Create(int siteId)
        {
            return new StatusType { SiteId = siteId, Weight = MinFreeWeight(StatusTypeRepository.GetWeightsBySiteId(siteId)) };
        }

        private static int MinFreeWeight(IEnumerable<int> occupiedWeights)
        {
            return Enumerable.Range(0, MaxWeight + 1).Except(occupiedWeights).Min();
        }

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
