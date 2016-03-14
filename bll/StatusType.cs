using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class StatusType : EntityObject
    {
		private static readonly int MAX_WEIGHT = 100;

		[LocalizedDisplayName("Weight", NameResourceType = typeof(StatusTypeStrings))]
        public int Weight
        {
            get;
            set;
        }

		[LocalizedDisplayName("Color", NameResourceType = typeof(StatusTypeStrings))]
		[MaxLengthValidator(6, MessageTemplateResourceName = "ColorLengthExceeded", MessageTemplateResourceType = typeof(StatusTypeStrings))]
		[FormatValidator(Constants.RegularExpressions.RgbColor, MessageTemplateResourceName = "ColorInvalidFormat", MessageTemplateResourceType = typeof(StatusTypeStrings))]
		public string Color
		{
			get;
			set;
		}

		[MaxLengthValidator(6, MessageTemplateResourceName = "AltColorLengthExceeded", MessageTemplateResourceType = typeof(StatusTypeStrings))]
		[FormatValidator(Constants.RegularExpressions.RgbColor, MessageTemplateResourceName = "AltColorInvalidFormat", MessageTemplateResourceType = typeof(StatusTypeStrings))]
		[LocalizedDisplayName("AltColor", NameResourceType = typeof(StatusTypeStrings))]
		public string AltColor
		{
			get;
			set;
		}

        public int SiteId
        {
            get;
            set;
        }

		public bool BuiltIn { get; set; }		

        public override string EntityTypeCode
        {
            get
            {
				return Constants.EntityTypeCode.StatusType;
            }
        }

		public override int ParentEntityId
		{
			get
			{
				return SiteId;
			}
		}

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
			return new StatusType { SiteId = siteId, Weight = MinFreeWeight(StatusTypeRepository.GetWeightsBySiteId(siteId))};
		}

		private static int MinFreeWeight(IEnumerable<int> occupiedWeights)
		{
			return Enumerable.Range(0, MAX_WEIGHT +1).Except(occupiedWeights).Min();
		}

		public override void Validate()
		{
			RulesException<StatusType> errors = new RulesException<StatusType>();
			if (String.IsNullOrEmpty(Color) && !String.IsNullOrEmpty(AltColor))
				errors.ErrorFor(n => n.Color, StatusTypeStrings.ColorMissed);
			if (String.IsNullOrEmpty(AltColor) && !String.IsNullOrEmpty(Color))
				errors.ErrorFor(n => n.AltColor, StatusTypeStrings.AltColorMissed);

			ValidateWeight(errors);
			base.Validate(errors);
			if (!errors.IsEmpty)
				throw errors;
		}

		private void ValidateWeight(RulesException<StatusType> errors)
		{
			if (StatusTypeRepository.GetWeightsBySiteId(SiteId, Id).Contains(Weight))
				errors.ErrorFor(n => n.Weight, StatusTypeStrings.WeightIsInUse);
		}
    }

}
