using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ContentConstraintRepository
    {
        internal static IEnumerable<ContentConstraint> GetConstraintsByContentId(int id)
        {
            return MapperFacade.ContentConstraintMapper.GetBizList(QPContext.EFContext.ContentConstraintSet.Include("Rules").Where(s => s.ContentId == (decimal)id).ToList());
        }

        internal static ContentConstraint GetConstraintByFieldId(int id)
        {
            ContentConstraint result = null;
            var constraintId = QPContext.EFContext.ContentConstraintRuleSet.Where(s => s.FieldId == (decimal)id).Select(s => s.ConstraintId).SingleOrDefault();
            if (constraintId > 0)
            {
                result = MapperFacade.ContentConstraintMapper.GetBizObject(QPContext.EFContext.ContentConstraintSet.Include("Rules").SingleOrDefault(s => s.Id == constraintId));
            }
            return result;
        }

        internal static ContentConstraint Save(ContentConstraint constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException(nameof(constraint));
            }

            if (!constraint.IsNew)
            {
                throw new ArgumentException("Метод вызван для существующего в БД ContentConstraint");
            }

            // Сохраняем ограничение только если есть правила
            if (constraint.Rules != null && constraint.Rules.Count > 0)
            {
                var ccDal = MapperFacade.ContentConstraintMapper.GetDalObject(constraint);
                ccDal = DefaultRepository.SimpleSave(ccDal);
                var newContraint = MapperFacade.ContentConstraintMapper.GetBizObject(ccDal);
                return newContraint;
            }

            return constraint;
        }

        internal static ContentConstraint Update(ContentConstraint constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException(nameof(constraint));
            }

            if (constraint.IsNew)
            {
                throw new ArgumentException("Метод вызван для несуществующего в БД ContentConstraint");
            }

            // если нет правил, то удалить ограничение
            if (constraint.Rules == null || constraint.Rules.Count == 0)
            {
                Delete(constraint);
                return null;
            }

            var ccDal = QPContext.EFContext.ContentConstraintSet.Single(d => d.Id == constraint.Id);
            ccDal.Rules.Load();
            IEnumerable<ContentConstraintRuleDAL> ruleDalList = ccDal.Rules.ToArray();

            // удалить все правила которые уже есть
            DefaultRepository.SimpleDelete(ruleDalList);

            // создать новые записи для правил
            foreach (var rule in constraint.Rules)
            {
                rule.ConstraintId = constraint.Id;
            }

            var newDalList = MapperFacade.ContentConstraintRuleMapper.GetDalList(constraint.Rules.ToList());
            DefaultRepository.SimpleSave(newDalList.AsEnumerable());

            return MapperFacade.ContentConstraintMapper.GetBizObject(ccDal);
        }

        public static void Delete(ContentConstraint constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException(nameof(constraint));
            }

            if (constraint.IsNew)
            {
                throw new ArgumentException("Метод вызван для несуществующего в БД ContentConstraint");
            }

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
