using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
	internal class ContentConstraintRepository
	{
		internal static IEnumerable<ContentConstraint> GetConstraintsByContentId(int id)
		{
			return MappersRepository.ContentConstraintMapper.GetBizList(
                QPContext.EFContext.ContentConstraintSet
                    .Include("Rules")
                    .Where(s => s.ContentId == (decimal)id)
                    .ToList()
            );
		}

        internal static ContentConstraint GetConstraintByFieldId(int id)
        {
            ContentConstraint result = null;
            decimal constraintId = QPContext.EFContext.ContentConstraintRuleSet.Where(s => s.FieldId == (decimal)id).Select(s => s.ConstraintId).SingleOrDefault();
            if (constraintId > 0)
            {
                result = MappersRepository.ContentConstraintMapper.GetBizObject(
                    QPContext.EFContext.ContentConstraintSet
                        .Include("Rules")
                        .Where(s => s.Id == constraintId)
                        .SingleOrDefault()
                );
            }
            return result;

        }

		internal static ContentConstraint Save(ContentConstraint constrain)
		{			
			if (constrain == null)
				throw new ArgumentNullException("constrain");
			if (!constrain.IsNew)
				throw new ArgumentException("Метод вызван для существующего в БД ContentConstraint");

			// Сохраняем ограничение только если есть правила
			if (constrain.Rules != null && constrain.Rules.Count > 0)
			{				
				ContentConstraintDAL ccDal = MappersRepository.ContentConstraintMapper.GetDalObject(constrain);				
				// добавить в БД запись ContentConstraint
				ccDal = DefaultRepository.SimpleSave(ccDal);				
				ContentConstraint newContraint = MappersRepository.ContentConstraintMapper.GetBizObject(ccDal);
				
				return newContraint;
			}
			else
				return constrain;
		}

		internal static ContentConstraint Update(ContentConstraint constraint)
		{
			if (constraint == null)
				throw new ArgumentNullException("constrain");
			if (constraint.IsNew)
				throw new ArgumentException("Метод вызван для несуществующего в БД ContentConstraint");

			// если нет правил, то удалить ограничение
			if (constraint.Rules == null || constraint.Rules.Count == 0)
			{
				Delete(constraint);
				return null;
			}
			else
			{

				ContentConstraintDAL ccDal = QPContext.EFContext.ContentConstraintSet.Single(d => d.Id == constraint.Id);
				ccDal.Rules.Load();
				IEnumerable<ContentConstraintRuleDAL> ruleDalList = ccDal.Rules.ToArray();

				// удалить все правила которые уже есть
				DefaultRepository.SimpleDelete(ruleDalList);

				// создать новые записи для правил
				foreach (var rule in constraint.Rules)
				{
					rule.ConstraintId = constraint.Id;					
				}
				var newDalList = MappersRepository.ContentConstraintRuleMapper.GetDalList(constraint.Rules.ToList());
				DefaultRepository.SimpleSave(newDalList.AsEnumerable());

				return MappersRepository.ContentConstraintMapper.GetBizObject(ccDal);
			}
		}

		public static void Delete(ContentConstraint constraint)
		{
			if (constraint == null)
				throw new ArgumentNullException("constraint");
			if (constraint.IsNew)
				throw new ArgumentException("Метод вызван для несуществующего в БД ContentConstraint");

			// удалить ограничение
			DefaultRepository.Delete<ContentConstraintDAL>(constraint.Id);
		}
        internal static void CopyContentConstrainRules(string relationsBetweenConstraintsXml, string relationsBetweenAttributesXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentConstrainRules(QPConnectionScope.Current.DbConnection, relationsBetweenConstraintsXml, relationsBetweenAttributesXml);
            }
        }
	}
}
