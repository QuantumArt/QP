using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository.Results;
using System.Data;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Repository
{
	internal class VirtualFieldRepository
	{
		internal static Field Save(Field item)
		{
			DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Field, item);
			Field newItem = DefaultRepository.Save<Field, FieldDAL>(item);
			DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Field);
			return newItem;			
		}

		internal static Field Update(Field item)
		{
			Field newItem = DefaultRepository.Update<Field, FieldDAL>(item);
			return newItem;
		}


		/// <summary>
		/// Определяет, существуют ли виртуальные поля в Join-контентах построенные на текущем поле
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		internal static bool JoinVirtualChildrenFieldsExist(Field field)
		{
			return QPContext.EFContext.FieldSet
					.Where(f => f.PersistentId != null && f.PersistentId.Value == field.Id)
					.Any();			
		}
		

		/// <summary>
		/// Удалить записи в таблице union_contents для всех полей union-контента
		/// </summary>
		/// <param name="unionSourceContentIDs"></param>
		internal static void RemoveUnionAttrs(Content unionContent)
		{			
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveUnionAttrsByUnionContent(scope.DbConnection, unionContent.Id);
			}	
		}

		/// <summary>
		/// Удалить записи в таблице union_contents для базовых полей
		/// </summary>
		/// <param name="unionSourceContentIDs"></param>
		internal static void RemoveUnionAttrs(IEnumerable<int> baseFieldIds)
		{			
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveUnionAttrsByBaseFields(scope.DbConnection, baseFieldIds);
			}
		}		

		internal static void RebuildUnionAttrs(Content unionContent)
		{
			var dbUnionContent = ContentRepository.GetById(unionContent.Id);
			var baseFields = VirtualContentRepository.GetFieldsOfContents(dbUnionContent.UnionSourceContentIDs)
				.Select(f => new { Id = f.Id, Name = f.Name.ToLowerInvariant() })
				.ToArray();
			var virtualFields = dbUnionContent.Fields
				.Select(f => new { Id = f.Id, Name = f.Name.ToLowerInvariant() })
				.ToArray();;

			IEnumerable<UnionAttr> newUnionAttr =
				from sf in baseFields
				join uf in virtualFields on sf.Name equals uf.Name
				select new UnionAttr
				{
					VirtualFieldId = uf.Id,
					BaseFieldId = sf.Id
				};
			
			// сохранить новые привязки
			IEnumerable<UnionAttrDAL> records = MappersRepository.UnionAttrMapper.GetDalList(newUnionAttr.ToList());
			using (var scope = new QPConnectionScope())
			{
				Common.BatchInsertUnionAttrs(scope.DbConnection, records);
			}			
		}

		/// <summary>
		/// Получает записи в таблице union_contents
		/// </summary>
		/// <param name="virtualFieldIds"></param>
		//internal static IEnumerable<UnionAttr> GetUnionAttrs(int contentId)
		//{
		//    var decimalContentId = Converter.ToDecimal(contentId);			

		//    //var dbcontext = QPContext.EFContext;
		//    //var fldMapper = MappersRepository.FieldMapper;			
			
		//    //var query =				
		//    //    from vf in dbcontext.FieldSet
		//    //    join ua in dbcontext.UnionAttrSet on vf.Id equals ua.VirtualFieldId
		//    //    join bf in dbcontext.FieldSet on ua.UnionFieldId equals bf.Id
		//    //    where vf.ContentId == decimalContentId
		//    //    select new
		//    //    {
		//    //        VirtualField = vf,
		//    //        BaseField = bf
		//    //    };
			
		//    //var r1 = query
		//    //    .ToArray()
		//    //    .Select(r => new UnionAttr 
		//    //    {
		//    //        BaseField = fldMapper.GetBizObject(r.BaseField),
		//    //        BaseFieldId = Converter.ToInt32(r.BaseField.Id),

		//    //        VirtualField = fldMapper.GetBizObject(r.VirtualField),
		//    //        VirtualFieldId = Converter.ToInt32(r.VirtualField.Id)
		//    //    })
		//    //    .ToArray();

		//    return MappersRepository.UnionAttrMapper.GetBizList(
		//        QPContext.EFContext.UnionAttrSet
		//            .Include("VirtualField")
		//            .Include("UnionField")
		//            .Where(a => a.VirtualField.ContentId == decimalContentId)
		//            .ToList()
		//    );			
		//}


		private class VirtualFieldRelationStorageItem
		{
			public int ScopeCount { get; set; }

			public IEnumerable<VirtualFieldsRelation> Data { get; set; }
		}
		
		/// <summary>
		/// Хранит данные о связи полей в памяти
		/// </summary>
		private class VirtualFieldRelationsStorage : IDisposable
		{
			[ThreadStatic]
			private static Dictionary<int, VirtualFieldRelationStorageItem> storage;

			[ThreadStatic]
			private static Queue<int> contentIds;
			
			public VirtualFieldRelationsStorage(int rootContentId)
			{
				if (storage == null)
					storage = new Dictionary<int, VirtualFieldRelationStorageItem>();
				
				if (contentIds == null)
					contentIds = new Queue<int>();

				contentIds.Enqueue(rootContentId);
				VirtualFieldRelationStorageItem item;

				if (!storage.ContainsKey(rootContentId))
				{
					item = new VirtualFieldRelationStorageItem() { ScopeCount = 1 };
					storage.Add(rootContentId, item);
					using (var scope = new QPConnectionScope())
					{
						DataTable dt = Common.LoadVirtualFieldsRelations(scope.DbConnection, rootContentId);
						item.Data = MappersRepository.VirtualFieldsRelationMapper.GetBizList(dt.AsEnumerable().ToList()).Distinct().ToArray();
					}
				}
				else
				{
					storage[rootContentId].ScopeCount++;
				}
			}					

			public void Dispose()
			{
				int rootContentId = contentIds.Dequeue();
				if (!contentIds.Any())
					contentIds = null;

				if (storage != null && storage.ContainsKey(rootContentId))
				{
					// уменьшаем счетчик вложенных scope
					storage[rootContentId].ScopeCount--;
					// если это последний, то все очищаем
					
					if (storage[rootContentId].ScopeCount == 0)
						storage.Remove(rootContentId);

					if (!storage.Keys.Any())
						storage = null;						
				}
			}

			public static IEnumerable<VirtualFieldsRelation> Data
			{
				get
				{
					int currentRootContentId = contentIds.Peek();
					return (storage != null && storage.ContainsKey(currentRootContentId)) ? storage[currentRootContentId].Data : null;
				}
			}
	
		}

		public static IDisposable LoadVirtualFieldsRelationsToMemory(int rootContentId)
		{
			return new VirtualFieldRelationsStorage(rootContentId);
		}

		/// <summary>
		/// Возвращает информацию о дочерних витуальных полях
		/// </summary>
		/// <param name="rootFieldId"></param>
		/// <returns></returns>
		public static IEnumerable<VirtualFieldsRelation> GetVirtualSubFields(IEnumerable<int> rootFieldId)
		{
			if (VirtualFieldRelationsStorage.Data == null)
				throw new ApplicationException("GetVirtualSubFields is invoke before LoadVirtualFieldsRelationsToMemory");

			if (!rootFieldId.Any())
			    return Enumerable.Empty<VirtualFieldsRelation>();

			using (var scope = new QPConnectionScope())
			{
				DataTable dt = Common.GetVirtualSubFields(scope.DbConnection, rootFieldId);
				List<VirtualFieldsRelation> result = MappersRepository.VirtualFieldsRelationMapper.GetBizList(dt.AsEnumerable().ToList());
				result.AddRange(VirtualFieldRelationsStorage.Data);
				return result
					.Where(r => rootFieldId.Contains(r.BaseFieldId))
					.Distinct()
					.ToArray();
			}						
		}

		public static IEnumerable<VirtualFieldsRelation> GetVirtualBaseFieldIDs(IEnumerable<int> subFieldIds)
		{
			List<VirtualFieldsRelation> result = new List<VirtualFieldsRelation>(VirtualFieldRelationsStorage.Data.Where(r => subFieldIds.Contains(r.VirtualFieldId)));
			using (var scope = new QPConnectionScope())
			{
				DataTable dt = Common.GetVirtualBaseFieldIDs(scope.DbConnection, subFieldIds);
				result.AddRange(MappersRepository.VirtualFieldsRelationMapper.GetBizList(dt.AsEnumerable().ToList()));
			}			
			return result.Distinct().ToArray();
		}

		/// <summary>
		/// Получить количество полей связей в Union_ATTR для union-полей
		/// </summary>
		/// <param name="rootFieldId"></param>
		/// <returns></returns>
		public static IEnumerable<UnionFieldRelationCount> GetUnionFieldRelationCount(IEnumerable<int> unionFieldIds)
		{
			if (!unionFieldIds.Any())
				return Enumerable.Empty<UnionFieldRelationCount>();

			using (var scope = new QPConnectionScope())
			{
				DataTable dt = Common.GetUnionFieldRelationCount(scope.DbConnection, unionFieldIds);
				var result = MappersRepository.UnionFieldRelationCountMapper.GetBizList(dt.AsEnumerable().ToList());
				return result;
			}
		}

		/// <summary>
		/// Обновить привязку базовых полей к UQ-контенту
		/// </summary>
		/// <param name="dbContent"></param>
		internal static void RebuildUserQueryAttrs(Content dbContent)
		{
			dbContent = ContentRepository.GetById(dbContent.Id);
			// определить какие контенты используются
			// и построить словарь используемых контентов
			IEnumerable<int> usedContentIds = dbContent.UserQueryContentViewSchema.SelectUniqContentIDs();
			IEnumerable<Content> usedContents = ContentRepository.GetList(usedContentIds);
			Dictionary<int, Dictionary<string, int>> usedContentDict = new Dictionary<int, Dictionary<string, int>>();
			foreach (var uc in usedContents)
			{
				usedContentDict.Add(uc.Id, new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase));
				foreach (var f in uc.Fields)
				{
					usedContentDict[uc.Id].Add(f.Name, f.Id);
				}
			}

			IEnumerable<UserQueryAttr> userQueryAttr = dbContent.UserQueryContentViewSchema
				.Where(c => c.ContentId.HasValue) // только колонки из контентов
				.Where(c => usedContentDict[c.ContentId.Value].ContainsKey(c.ColumnName))
				.Select(c => new UserQueryAttr
				{
					UserQueryContentId = dbContent.Id,
					BaseFieldId = usedContentDict[c.ContentId.Value][c.ColumnName]
				});

			IEnumerable<UserQueryAttrsDAL> records = MappersRepository.UserQueryAttrMapper.GetDalList(userQueryAttr.ToList()).AsEnumerable();
			using (var scope = new QPConnectionScope())
			{
				Common.BatchInsertUserQueryAttrs(scope.DbConnection, records);
			}
		}
		
		
		internal static void RemoveUserQueryAttrs(Content dbContent)
		{			
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveUserQueryAttrs(scope.DbConnection, dbContent.Id);
			}	
		}


		/// <summary>
		/// Для виртуального поля, возвращает ID всех реальных базовых полей
		/// </summary>
		/// <param name="virtualFieldId"></param>
		/// <returns></returns>
		internal static IEnumerable<int> GetRealBaseFieldIds(int virtualFieldId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.GetRealBaseFieldIds(scope.DbConnection, virtualFieldId);
			}
		}
	}
}
