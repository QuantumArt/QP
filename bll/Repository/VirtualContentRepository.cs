using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Constants;
using System.Data.SqlClient;
using System.Transactions;

namespace Quantumart.QP8.BLL.Repository
{
	internal class VirtualContentRepository
	{		
		/// <summary>
		/// Добавляет новый виртуальный контент
		/// </summary>
		/// <param name="content">информация о контенте</param>
		/// <returns>информация о контенте</returns>
		internal static Content Save(Content content)
		{
			DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Content, content);
			Content newContent = DefaultRepository.Save<Content, ContentDAL>(content);
			DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Content);
			return newContent;
		}

		/// <summary>
		/// Обновляет информацию о контенте
		/// </summary>
		/// <param name="content">информация о контенте</param>
		/// <returns>информация о контенте</returns>
		internal static Content Update(Content content)
		{
			Content newContent = DefaultRepository.Update<Content, ContentDAL>(content);
			return newContent;
		}

		/// <summary>
		/// Возвращает данные о виртуальных полях JOIN-контента
		/// </summary>
		/// <param name="contentId"></param>
		/// <returns></returns>
		internal static IEnumerable<VirtualFieldData> GetJoinFieldData(int contentId)
		{
			// Получить данные из БД
			using (var scope = new QPConnectionScope())
			{
				DataTable dt = Common.GetVirtualFieldData(scope.DbConnection, contentId);

				// Транформировать в Biz коллекцию			
				var result = MappersRepository.VirtualFieldDataMapper.GetBizList(dt.AsEnumerable().ToList());

				return result;
			}
		}

		// Возвращает список join-контентов которые содержат виртуальные поля построенные на основе полей родительского контента			
		internal static IEnumerable<Content> GetJoinRelatedContents(Content parentContent)
		{
			IEnumerable<decimal> baseFieldIds = Converter.ToDecimalCollection(parentContent.Fields.Select(f => f.Id)).Distinct().ToArray();
			IEnumerable<int> joinRelatedContentIDs = Converter.ToInt32Collection(
					QPContext.EFContext.FieldSet
					.Where(f => f.PersistentId != null && baseFieldIds.Contains(f.PersistentId.Value))
					.Select(f => f.ContentId)
					.Distinct()
					.ToArray()
			);
			return ContentRepository.GetList(joinRelatedContentIDs)
				.Where(c => c.VirtualType == VirtualType.Join)
				.ToArray();
		}

		/// <summary>
		/// Выполнить DDL запрос на создание View
		/// </summary>
		/// <param name="viewCreateDDL"></param>
		internal static void RunCreateViewDDL(string viewCreateDDL)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.ExecuteSql(scope.DbConnection, viewCreateDDL);
			}
		}

		internal static void CreateUnitedView(int contentId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CreateUnitedView(scope.DbConnection, contentId);
			}
		}

		internal static void CreateFrontedViews(int contentId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CreateFrontedViews(scope.DbConnection, contentId);
			}
		}

		internal static void DropView(string viewName)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.DropView(scope.DbConnection, viewName);
			}
		}

		internal static void RefreshView(string viewName)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.RefreshView(scope.DbConnection, viewName);
			}
		}

		/// <summary>
		/// Возвращает список контентов на основе которых можно строить виртуальные контетны типа join
		/// </summary>
		/// <param name="siteId"></param>
		/// <returns></returns>
		internal static IEnumerable<ListItem> GetAcceptableContentForVirtualJoin(int siteId)
		{
			return QPContext.EFContext.ContentSet
				.Where(c =>
						c.VirtualType == (decimal)VirtualType.None &&
						(
							c.SiteId == siteId ||
							(c.SiteId != siteId && c.IsShared)
						)
					  )
				.Select(c => new { Id = c.Id, Text = c.Name })
				.ToArray()
				.OrderBy(c => c.Text, StringComparer.InvariantCultureIgnoreCase)
				.Select(c => new ListItem() { Value = c.Id.ToString(), Text = c.Text })
				.ToArray();
		}

		/// <summary>
		/// Возвращает поля всех контентов id которых указаны
		/// </summary>
		/// <param name="contentIDs"></param>
		/// <returns></returns>
		internal static IEnumerable<Field> GetFieldsOfContents(IEnumerable<int> contentIDs)
		{
			var dContentIDs = Converter.ToDecimalCollection(contentIDs);
			return MappersRepository.FieldMapper.GetBizList(FieldRepository.DefaultFieldQuery
					.Where(f => dContentIDs.Contains(f.ContentId))
					.OrderBy(f => f.ContentId)
					.ToList()
			);
		}

		/// <summary>
		/// Возвращает контенты-источники для UNION 
		/// </summary>
		/// <param name="contentId"></param>
		/// <returns></returns>
		internal static IEnumerable<int> GetUnionSourceContents(int contentId)
		{
			return Converter.ToInt32Collection(QPContext.EFContext.UnionContentsSet
				.Where(r => r.VirtualContentId == contentId)
				.Select(r => r.UnionContentId)
				.Distinct()
				.ToArray());
		}

		internal static void ChangeUnionContentTriggerState(bool enable)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.ChangeTriggerState(scope.DbConnection, "ti_union_contents_auto_map_attrs", enable);
			}
		}

		/// <summary>
		/// Сохранить записи в таблице union_contents
		/// </summary>
		/// <param name="unionSourceContentIDs"></param>
		internal static void RecreateUnionSourcesInfo(Content virtualContent, IEnumerable<int> unionSourceContentIDs)
		{
			try
			{
				ChangeUnionContentTriggerState(false);				
				int virtualContentId = virtualContent.Id;

				// Удалить 
				var recToRemove = QPContext.EFContext.UnionContentsSet
					.Where(u => u.VirtualContentId == virtualContentId)
					.ToArray();
				DefaultRepository.SimpleDelete(recToRemove);

				// создать новые
				DefaultRepository.SimpleSave(unionSourceContentIDs.Select(usID => new UnionContentsDAL { VirtualContentId = virtualContentId, UnionContentId = usID }));
			}
			finally
			{
				ChangeUnionContentTriggerState(true);
			}
		}

		internal static void RemoveUnionSourcesInfo(Content virtualContent)
		{
			int virtualContentId = virtualContent.Id;
			// Удалить 
			var recToRemove = QPContext.EFContext.UnionContentsSet
				.Where(u => u.VirtualContentId == virtualContentId)
				.ToArray();
			DefaultRepository.SimpleDelete(recToRemove);
		}

		/// <summary>
		/// Проверяет текст запроса на корректность
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="sqlQuery"></param>
		internal static bool IsQueryQueryCorrect(string userQuery, out string errorMessage)
		{
			errorMessage = null;
			using (new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
			{
				try
				{
					string viewName = String.Format("uq_v_test_{0}", DateTime.Now.Ticks.ToString());
					string createTestViewSql = String.Format("CREATE VIEW [dbo].{0} AS {1}", viewName, userQuery);

					using (SqlConnection connect = new SqlConnection(QPContext.CurrentDBConnectionString))
					{
						connect.Open();
						Common.ExecuteSql(connect, createTestViewSql);
						Common.DropView(connect, viewName);						
					}										
					return true;
				}
				catch (SqlException ex)
				{
					errorMessage = ex.ErrorsToString();					
					return false;
				}
			}					
		}

		/// <summary>
		/// Возвращает информацию о столбцах запроса
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="sqlQuery"></param>
		internal static IEnumerable<UserQueryColumn> GetQuerySchema(string sqlQuery)
		{
			using (var scope = new QPConnectionScope())
			{								

				string viewName = String.Format("uq_v_test_{0}", DateTime.Now.Ticks.ToString());
				string createTestViewSql = String.Format("CREATE VIEW [dbo].{0} AS {1}", viewName, sqlQuery);
				RunCreateViewDDL(createTestViewSql);
				DataTable dtTU = Common.GetViewColumnUsage(scope.DbConnection, viewName);
				DropView(viewName);

				IEnumerable<UserQueryColumn> result = DataTableToUserQueryColumns(dtTU);

				return result;
			}
		}

		/// <summary>
		/// Возвращает информацию о столбцах View
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="sqlQuery"></param>
		internal static IEnumerable<UserQueryColumn> GetViewSchema(string viewName)
		{
			using (var scope = new QPConnectionScope())
			{				
				DataTable dtTU = Common.GetViewColumnUsage(scope.DbConnection, viewName);
				IEnumerable<UserQueryColumn> result = DataTableToUserQueryColumns(dtTU);

				return result;
			}
		}

		private static IEnumerable<UserQueryColumn> DataTableToUserQueryColumns(DataTable dt)
		{
			return dt.AsEnumerable()
				.Select(r => new UserQueryColumn
				{
					ColumnName = r.Field<string>("ColumnName"),
					DbType = r.Field<string>("DbType"),
					TableName = r.Field<string>("TableName"),
					TableDbType = r.Field<string>("TableDbType"),
					NumericScale = r.Field<int?>("NumericScale"),
					CharMaxLength = r.Field<int?>("CharMaxLength"),
				})
				.ToArray();
		}

		/// <summary>
		/// Сохранить записи в таблице union_contents
		/// </summary>
		/// <param name="unionSourceContentIDs"></param>
		internal static void RecreateUserQuerySourcesInfo(Content uqVirtualContent)
		{			
			int virtualContentId = uqVirtualContent.Id;

			// Удалить записи для контента
			RemoveUserQuerySourcesInfo(uqVirtualContent);

			// создать новые
			DefaultRepository.SimpleSave(
				uqVirtualContent.UserQueryContentViewSchema
				.SelectUniqContentIDs()
				.Select(usID => new UserQueryContentsDAL { VirtualContentId = virtualContentId, RealContentId = usID, IsIdSource = false })
			);			
		}

		internal static void RemoveUserQuerySourcesInfo(Content uqVirtualContent)
		{
			int virtualContentId = uqVirtualContent.Id;

			// Удалить записи для контента
			var recToRemove = QPContext.EFContext.UserQueryContentsSet
					.Where(u => u.VirtualContentId == virtualContentId)
					.ToArray();
			DefaultRepository.SimpleDelete(recToRemove);
		}

		/// <summary>
		/// Возвращает граф связей контентов
		/// </summary>
		/// <returns></returns>
		internal static Dictionary<int, int[]> GetContentRelationGraph()
		{
			using (var scope = new QPConnectionScope())
			{
				DataTable relationView = Common.GetVirtualContentRelations(scope.DbConnection);
				var graph = relationView.AsEnumerable()
					.GroupBy(r => r.Field<decimal>("BASE_CONTENT_ID"))
					.Select(g => new 
					{ 
						BaseContentID = g.Key, 
						ParentContentIDs = g.Select(vr => vr.Field<decimal>("VIRTUAL_CONTENT_ID")).ToArray()
					});
				Dictionary<int, int[]> result = new Dictionary<int, int[]>();
				foreach (var p in graph)
				{
					result.Add(Converter.ToInt32(p.BaseContentID), Converter.ToInt32Collection(p.ParentContentIDs).ToArray());
				}
				return result;
			}
		}

		internal static Field GetAcceptableBaseFieldForCloning(string fieldName, string contentIds, int virtualContentId, bool forNew)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> ids = Common.GetAcceptableBaseFieldIdsForCloning(scope.DbConnection, fieldName, contentIds, virtualContentId, forNew);
				if (ids.Any())
				{
					int id = ids.Select(vr => (int)vr.Field<decimal>("id")).First();
					return FieldRepository.GetById(id);
				}
				else
					return null;
			}
		}

		internal static IEnumerable<EntityObject> GetList(IEnumerable<int> IDs)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			return MappersRepository.ContentMapper
				.GetBizList(QPContext.EFContext.ContentSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);			
		}
	}
}
