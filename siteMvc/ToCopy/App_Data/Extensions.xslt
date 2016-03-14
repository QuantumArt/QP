<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">

<xsl:output method="text" />
    
<xsl:template match="schema">
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using System;
using System.Collections;
using System.Collections.Generic;

using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Web;
using Quantumart.QPublishing.Database;


    <xsl:if test="@namespace">
namespace <xsl:value-of select="@namespace" /> 
{
</xsl:if>

	<xsl:call-template name="LinqHelper" />
	
	<xsl:call-template name="DataContext" />

public static class UserExtensions
{
    <xsl:apply-templates select="content" mode="Filters"/>
}

    <xsl:call-template name="ListSelector" />

    <xsl:apply-templates select="content"  />


<xsl:if test="@namespace">
}
</xsl:if>

</xsl:template>



<xsl:template name="LinqHelper">
	public interface IQPContent
	{
		int Id { get; set; }
		int StatusTypeId { get; set; }
		StatusType StatusType { get; set; }
		bool StatusTypeChanged { get; set; }
		bool Visible { get; set; }
		bool Archive { get; set; }
		DateTime Created { get; set; }
		DateTime Modified { get; set; }
		int LastModifiedBy { get; set; }
	}

	public interface IQPLink
	{
		int Id { get; }
		int LinkedItemId { get; }
		int LinkId { get; }
		bool InsertWithArticle { get; set; }
		bool RemoveWithArticle { get; set; }
		void SaveInsertingInstruction();
		void SaveRemovingInstruction();
		string InsertingInstruction { get; }
		string RemovingInstruction { get; }
	}

	public abstract class QPEntityBase
	{
		protected <xsl:value-of select="@class" /> _InternalDataContext = LinqHelper.Context;
		
		public <xsl:value-of select="@class" /> InternalDataContext
		{
			get { return _InternalDataContext; }
			set { _InternalDataContext = value; }
		}
		
		public abstract void Detach();
		
		protected static EntityRef&lt;T&gt; Detach&lt;T&gt;(EntityRef&lt;T&gt; entity) where T : QPEntityBase
		{
			if (!entity.HasLoadedOrAssignedValue || entity.Entity == null)
				return new EntityRef&lt;T&gt;();
			entity.Entity.Detach();
			return new EntityRef&lt;T&gt;(entity.Entity);
		}
		
		protected static EntitySet&lt;T&gt; Detach&lt;T&gt;(EntitySet&lt;T&gt; set, Action&lt;T&gt; onAdd, Action&lt;T&gt; onRemove) where T : QPEntityBase
		{
			if (set == null || !set.HasLoadedOrAssignedValues)
				return new EntitySet&lt;T&gt;(onAdd, onRemove);
					
			var list = set.ToList();
			list.ForEach(t => t.Detach());
				
			var newSet = new EntitySet&lt;T&gt;(onAdd, onRemove);
			newSet.Assign(list);
			return newSet;
		}
		
		protected static Link&lt;T&gt; Detach&lt;T&gt;(Link&lt;T&gt; value)
		{
			if (!value.HasLoadedOrAssignedValue)
				return default(Link&lt;T&gt;);
					
			return new Link&lt;T&gt;(value.Value);
		}		
	}

	public abstract class QPLinkBase : QPEntityBase
	{

		private bool _insertWithArticle = false;
		public bool InsertWithArticle
		{
			get
			{
				return _insertWithArticle;
			}
			set
			{
				_insertWithArticle = value;
			}
		}
		
		private bool _removeWithArticle = false;
		public bool RemoveWithArticle
		{
			get
			{
				return _removeWithArticle;
			}
			set
			{
				_removeWithArticle = value;
			}
		}	
		
		private string _insertingInstruction;
		public string InsertingInstruction
		{
			get
			{
				if (String.IsNullOrEmpty(_insertingInstruction))
					SaveInsertingInstruction();
				return _insertingInstruction;
			}
		}
		
		
		private string _removingInstruction;
		public string RemovingInstruction
		{
			get
			{
				if (String.IsNullOrEmpty(_removingInstruction))
					SaveRemovingInstruction();
				return _removingInstruction;
			}
		}
		
		public void SaveRemovingInstruction()
		{
			_removingInstruction = String.Format("EXEC sp_executesql N'EXEC qp_delete_single_link @linkId, @itemId, @linkedItemId', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", this.LinkId, this.Id, this.LinkedItemId);
		}
		
		public void SaveInsertingInstruction()
		{
			_insertingInstruction = String.Format("EXEC sp_executesql N'EXEC qp_insert_single_link @linkId, @itemId, @linkedItemId', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", this.LinkId, this.Id, this.LinkedItemId);
		}
		
		public abstract int LinkId { get; }
		public abstract int Id { get; }
		public abstract int LinkedItemId { get; }
		
	
	}

	public abstract class QPContentBase : QPEntityBase
	{
		protected bool _StatusTypeChanged;
		public bool StatusTypeChanged
		{
			get { return _StatusTypeChanged; }
			set { _StatusTypeChanged = value; }
		}
        
		protected void HandlePropertyChangedEvent(object sender, PropertyChangedEventArgs a)
		{
			if (a.PropertyName == "StatusType")
			{
				_StatusTypeChanged = true;
			}
		}
	}
	
	public partial class StatusType: QPEntityBase
	{
		public override void Detach()
		{
				if (null == PropertyChanging)
					return;

				PropertyChanging = null;
				PropertyChanged = null;
		}
	}

	<xsl:choose><xsl:when test="@isPartial = 'false'">public</xsl:when><xsl:otherwise>internal</xsl:otherwise></xsl:choose> static partial class LinqHelper
{
	
	private static string _Key = Guid.NewGuid().ToString();
	private static string Key
	{
		get
		{
			return "LinqUtilDataContextKey " + _Key;
		}
	}
	
	[ThreadStatic]
	private	static <xsl:value-of select="@class" /> _context;
	private	static <xsl:value-of select="@class" /> InternalDataContext
	{
		get
		{
			if (HttpContext.Current	== null)
				return _context;
			else
				return (<xsl:value-of select="@class" />)HttpContext.Current.Items[Key];
			}
		set
		{
			if (HttpContext.Current	== null)
				_context = value;
			else
				HttpContext.Current.Items[Key] = value;
		}
	}
		
	public static <xsl:value-of select="@class" /> Context
	{
		get
		{
			if (InternalDataContext	== null)
			{
				InternalDataContext	=	<xsl:value-of select="@class" />.Create();
			}
			return InternalDataContext;
		}
	}
}

</xsl:template>

<xsl:template name="DataContext">

public partial class <xsl:value-of select="@class" />
{
	<xsl:choose>
	<xsl:when test="@dbIndependent = 'true'">
	
	private static string _DefaultSiteName = "<xsl:value-of select="@siteName" />";
	
	public static string DefaultSiteName { 
		get
		{
			return _DefaultSiteName;
		}
		set
		{
			_DefaultSiteName = value;
		}
	}
	
	private static string _DefaultConnectionString;

	public static string DefaultConnectionString { 
		get
		{
			if (_DefaultConnectionString == null)
			{
				var obj = <xsl:value-of select="@connectionStringObject" />["<xsl:value-of select="@connectionStringName" />"];
				if (obj == null)
					throw new ApplicationException("Connection string '<xsl:value-of select="@connectionStringName" />' is not specified");
				_DefaultConnectionString = obj.ConnectionString;
			}
			return _DefaultConnectionString;
		}
		set
		{
			_DefaultConnectionString = value;
		}
	}

	private static XmlMappingSource _DefaultXmlMappingSource;
        	
	public static XmlMappingSource DefaultXmlMappingSource {
		get
		{
			if (_DefaultXmlMappingSource == null)
			{
				_DefaultXmlMappingSource = GetDefaultXmlMappingSource(null);
			}
			return _DefaultXmlMappingSource;
		}
		set
		{
			_DefaultXmlMappingSource = value;
		}
	}

	public static XmlMappingSource GetDefaultXmlMappingSource(IDbConnection connection)
	{
		DBConnector dbc = (connection != null) ? new DBConnector(connection) : new DBConnector(DefaultConnectionString);
		return XmlMappingSource.FromXml(dbc.GetDefaultMapFileContents(dbc.GetSiteId(DefaultSiteName)));	
	}

	public static <xsl:value-of select="@class" /> Create(IDbConnection connection, string siteName, MappingSource mappingSource)
	{
		<xsl:value-of select="@class" /> ctx = new <xsl:value-of select="@class" />(connection, mappingSource);
		ctx.SiteName = siteName;
		ctx.ConnectionString = connection.ConnectionString;
		return ctx;
	}

	public static <xsl:value-of select="@class" /> Create(IDbConnection connection, string siteName)
	{
		return Create(connection, siteName, GetDefaultXmlMappingSource(connection));
	}

	public static <xsl:value-of select="@class" /> Create(IDbConnection connection)
	{
		return Create(connection, DefaultSiteName);
	}

	public static <xsl:value-of select="@class" /> Create(string connection, string siteName, MappingSource mappingSource) 
	{
		<xsl:value-of select="@class" /> ctx = new <xsl:value-of select="@class" />(connection, mappingSource);
		ctx.SiteName = siteName;
		ctx.ConnectionString = connection;
		return ctx;
	}
	
	public static <xsl:value-of select="@class" /> Create(string siteName, MappingSource mappingSource) 
	{
		return Create(DefaultConnectionString, siteName, mappingSource);
	}
	
	public static <xsl:value-of select="@class" /> Create(string connection, string siteName) 
	{
		return Create(connection, siteName, DefaultXmlMappingSource);
	}
	
	public static <xsl:value-of select="@class" /> Create(string connection) 
	{
		return Create(connection, DefaultSiteName);
	}
	
	public static <xsl:value-of select="@class" /> Create() 
	{
		return Create(DefaultConnectionString);
	}
	
	public string ConnectionString { get; private set; }
	
	private string _SiteName;
	public string SiteName 
	{ 
		get 
		{ 
			return _SiteName; 
		} 
		set
		{
			if (!String.Equals(_SiteName, value, StringComparison.InvariantCultureIgnoreCase))
			{
				_SiteName = value;
				SiteId = Cnn.GetSiteId(_SiteName);
				LoadSiteSpecificInfo();
			}
		}
	}
	
	public Int32 SiteId { get; private set; }
	
	</xsl:when>
	<xsl:otherwise>
	
	public static <xsl:value-of select="@class" /> Create() 
	{
		return new <xsl:value-of select="@class" />();
	}
	
	partial void OnCreated()
	{
		LoadSiteSpecificInfo();
	}
	
	public Int32 SiteId
	{
		get
		{
			return <xsl:value-of select="@siteId"/>;
		}
	}
	
	public string ConnectionString
	{
		get
		{
			var obj = <xsl:value-of select="@connectionStringObject" />["<xsl:value-of select="@connectionStringName" />"];
			if (obj == null)
				throw new ApplicationException("Connection string '<xsl:value-of select="@connectionStringName" />' is not specified");
			return obj.ConnectionString;
		}
	}

	
	</xsl:otherwise>
	</xsl:choose>
	
	<xsl:variable name="liveSiteUrlVariable">
		<xsl:choose>
			<xsl:when test="@useLongUrls = 'true'">LiveSiteUrl</xsl:when>
			<xsl:otherwise>LiveSiteUrlRel</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="stageSiteUrlVariable">
		<xsl:choose>
			<xsl:when test="@useLongUrls = 'true'">StageSiteUrl</xsl:when>
			<xsl:otherwise>StageSiteUrlRel</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

  <xsl:variable name="siteUrlVariable">
      <xsl:choose>
          <xsl:when test="@dbIndependent = 'true'">(Cnn.IsStage) ? <xsl:value-of select="$stageSiteUrlVariable"/> : <xsl:value-of select="$liveSiteUrlVariable"/></xsl:when>
          <xsl:when test="@forStage = 'true' "><xsl:value-of select="$stageSiteUrlVariable"/></xsl:when>
          <xsl:otherwise><xsl:value-of select="$liveSiteUrlVariable"/></xsl:otherwise>
      </xsl:choose> 
  </xsl:variable>

  <xsl:variable name="uploadUrlVariable">
      <xsl:choose>
          <xsl:when test="@useLongUrls = 'true'">LongUploadUrl</xsl:when>
          <xsl:otherwise>ShortUploadUrl</xsl:otherwise>
      </xsl:choose>
  </xsl:variable>

	private DBConnector _cnn;
	public DBConnector Cnn
	{
		get 
		{
			if (_cnn == null) 
			{
				_cnn = (Connection != null) ? new DBConnector(Connection, Transaction) : new DBConnector(ConnectionString);
				<xsl:if test="@cacheData = 'false'" >_cnn.CacheData = false;</xsl:if>
				_cnn.UpdateManyToOne = false;
				_cnn.ThrowNotificationExceptions = false;
			}
			return _cnn;
		}
	}
  
	public static bool RemoveUploadUrlSchema = false;
	private bool _ShouldRemoveSchema = false;
  
  public bool ShouldRemoveSchema { get { return _ShouldRemoveSchema; } set { _ShouldRemoveSchema = value; }}
  
	public void LoadSiteSpecificInfo()
	{
		if (RemoveUploadUrlSchema &amp;&amp; !_ShouldRemoveSchema)
		{
				_ShouldRemoveSchema = RemoveUploadUrlSchema;
		}
    
		LiveSiteUrl = Cnn.GetSiteUrl(SiteId, true);
		LiveSiteUrlRel = Cnn.GetSiteUrlRel(SiteId, true);
		StageSiteUrl = Cnn.GetSiteUrl(SiteId, false);
		StageSiteUrlRel = Cnn.GetSiteUrlRel(SiteId, false);
		LongUploadUrl = Cnn.GetImagesUploadUrl(SiteId, false, _ShouldRemoveSchema);
		ShortUploadUrl = Cnn.GetImagesUploadUrl(SiteId, true, _ShouldRemoveSchema);
		PublishedId = Cnn.GetMaximumWeightStatusTypeId(SiteId);
	}
	
	public string SiteUrl
	{
		get
		{
			return <xsl:value-of select="$siteUrlVariable"/>;
		}
	}
	
	public string UploadUrl
	{
		get
		{
			return <xsl:value-of select="$uploadUrlVariable"/>;
		}
	}
	
	public string LiveSiteUrl { get; private set; }
	
	public string LiveSiteUrlRel { get; private set; }
	
	public string StageSiteUrl { get; private set; }
	
	public string StageSiteUrlRel { get; private set; }
	
	public string LongUploadUrl { get; private set; }
	
	public string ShortUploadUrl { get; private set; }
	
	public Int32 PublishedId { get; private set; }
	
	
	private string uploadPlaceholder = "&lt;%=upload_url%&gt;";
	private string sitePlaceholder = "&lt;%=site_url%&gt;";
	
	public string ReplacePlaceholders(string input)
	{
		string result = input;
		if (result != null)
		{
			result = result.Replace(uploadPlaceholder, UploadUrl);
			result = result.Replace(sitePlaceholder, SiteUrl);
		}
		return result;
	}
	
	public string ReplaceUrls(string input)
	{
		string result = input;
		if (result != null)
		{
			result = result.Replace(LongUploadUrl, uploadPlaceholder);
			result = result.Replace(ShortUploadUrl, uploadPlaceholder);
			result = result.Replace(LiveSiteUrl, sitePlaceholder);
			result = result.Replace(StageSiteUrl, sitePlaceholder);
			if (!String.Equals(LiveSiteUrlRel, "/")) result = result.Replace(LiveSiteUrlRel, sitePlaceholder);
			if (!String.Equals(StageSiteUrlRel, "/")) result = result.Replace(StageSiteUrlRel, sitePlaceholder);
		}
		return result;
	}
			

	public override void SubmitChanges(System.Data.Linq.ConflictMode failureMode)
	{
		Cnn.ExternalTransaction = Transaction;
		ChangeSet delta = GetChangeSet();
		
		foreach (var delete in delta.Deletes.OfType&lt;IQPLink&gt;().Where(n => !n.RemoveWithArticle))
		{
			delete.SaveRemovingInstruction();
		}
		
		foreach (var insert in delta.Inserts.OfType&lt;IQPLink&gt;().Where(n => !n.InsertWithArticle))
		{
			insert.SaveInsertingInstruction();
		}

	<xsl:if test="@sendNotifications = 'true'">
		HashSet&lt;int&gt; changedStatusesIds = new HashSet&lt;int&gt;(
			delta.Updates
			.OfType&lt;IQPContent&gt;()
			.Where(n => n.StatusTypeChanged)
			.Select(n => n.Id)
			.ToList()
		);

		foreach (var delete in delta.Deletes.OfType&lt;IQPContent&gt;())
		{
			Cnn.SendNotification(delete.Id, "for_remove");
		}
		
	</xsl:if>
		
		base.SubmitChanges(failureMode);
		
		foreach (var delete in delta.Deletes.OfType&lt;IQPLink&gt;().Where(n => !n.RemoveWithArticle))
		{
			Cnn.ProcessData(delete.RemovingInstruction);
		}
		
		foreach (var insert in delta.Inserts.OfType&lt;IQPLink&gt;().Where(n => !n.InsertWithArticle))
		{
			Cnn.ProcessData(insert.InsertingInstruction);
		}
	
	<xsl:if test="@sendNotifications = 'true'">		
		foreach (var insert in delta.Inserts.OfType&lt;IQPContent&gt;())
		{
			Cnn.SendNotification(insert.Id, "for_create");
		}

		foreach (var update in delta.Updates.OfType&lt;IQPContent&gt;())
		{
			Cnn.SendNotification(update.Id, "for_modify");
			if (changedStatusesIds.Contains(update.Id))
			{
				Cnn.SendNotification(update.Id, "for_status_changed");
			}
		}
	</xsl:if>	
		

	}

}
</xsl:template>

<xsl:template match="content" >
  <xsl:variable name="name" select="@mapped_name" />
  <xsl:variable name="site_id" select="parent::node()/@siteId" />
	<xsl:variable name="id" select="@id" />
	<xsl:variable name="context_name" select="parent::node()/@class" />
	partial class <xsl:value-of select="$name" /> : QPContentBase, IQPContent
	{
		
		public <xsl:value-of select="$name" />(<xsl:value-of select="$context_name" /> context) 
		{
			_InternalDataContext = context;
			OnCreated();
		}
		
		public void LoadStatusType()
		{
			_StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);		
		}
		
		public override void Detach()
		{
			if (null == PropertyChanging)
				return;

			PropertyChanging = null;
			PropertyChanged = null;
			
			<xsl:apply-templates select="attribute[@type='O2M']" mode="DetachAssociation">
			</xsl:apply-templates>
			<xsl:apply-templates select="attribute[@type='M2O']" mode="DetachExplicitBackAssociation">
			</xsl:apply-templates>

			<xsl:if test="@virtual = '0'">
				<xsl:apply-templates select="attribute[@type='M2M']" mode="DetachLink" />
			</xsl:if>
			<xsl:apply-templates select="parent::node()/content/attribute[@type='O2M' and @has_m2o='false' and @related_content_id=$id]" mode="DetachImplicitBackAssociation" />
			this._StatusType = Detach(this._StatusType);
		}
		
		partial void OnCreated()
		{
			if (_InternalDataContext == null)
				_InternalDataContext = LinqHelper.Context;
			_Visible = true;
			_Archive = false;
			_StatusTypeId = _InternalDataContext.PublishedId;
			_StatusTypeChanged = false;
			PropertyChanged += HandlePropertyChangedEvent;
		}
		
		partial void OnLoaded()
		{
<xsl:if test="parent::node()/@replaceUrls = 'true'" ><xsl:apply-templates select="attribute[@type = 'String' or @type = 'VisualEdit' or @type = 'Textbox']" /></xsl:if>
		}
		
	<xsl:apply-templates select="attribute[@type = 'Image' or @type = 'File' or @type = 'Dynamic Image']" mode="Files"/>
	<xsl:apply-templates select="attribute[@type = 'Numeric' or @type = 'Boolean' or @type = 'DateTime' or @type = 'Date' or @type = 'Time']" mode="ValueTypes"/>
	}

</xsl:template>

<xsl:template match="attribute" mode="Files">

	<xsl:variable name="attribute_id">
		<xsl:choose>
			<xsl:when test="parent::node()/parent::node()/@dbIndependent = 'true'">InternalDataContext.Cnn.GetAttributeIdByNetNames(InternalDataContext.SiteId, "<xsl:value-of select="parent::node()/@mapped_name" />", "<xsl:value-of select="@mapped_name" />")</xsl:when>
			<xsl:otherwise><xsl:value-of select="@id" /></xsl:otherwise>
		</xsl:choose> 
	</xsl:variable>

		private string _<xsl:value-of select="@mapped_name"/>Url;
		
        public string <xsl:value-of select="@mapped_name"/>Url
        {
            get 
            {
                if (String.IsNullOrEmpty(<xsl:value-of select="@mapped_name"/>))
					return String.Empty;
				else 
				{
					if (_<xsl:value-of select="@mapped_name"/>Url == null)
						_<xsl:value-of select="@mapped_name"/>Url = String.Format(@"{0}/{1}", InternalDataContext.Cnn.GetUrlForFileAttribute(<xsl:value-of select="$attribute_id"/>, true, _InternalDataContext.ShouldRemoveSchema), <xsl:value-of select="@mapped_name"/>);
					return _<xsl:value-of select="@mapped_name"/>Url;
				}
            }
        }
		
		private string _<xsl:value-of select="@mapped_name"/>UploadPath;
		
        public string <xsl:value-of select="@mapped_name"/>UploadPath
        {
            get 
            {
                if (_<xsl:value-of select="@mapped_name"/>UploadPath == null)
					_<xsl:value-of select="@mapped_name"/>UploadPath = InternalDataContext.Cnn.GetDirectoryForFileAttribute(<xsl:value-of select="$attribute_id"/>);
				
				return (_<xsl:value-of select="@mapped_name"/>UploadPath);
            }
        }

</xsl:template>
	
<xsl:template match="attribute" mode="ValueTypes">
	<xsl:variable name="type">
			<xsl:call-template name="GetType">
				<xsl:with-param name="type" select="@type" />
				<xsl:with-param name="size" select="@size" />
				<xsl:with-param name="is_long" select="@is_long" />
			</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="signature">
		<xsl:value-of select="concat($type, ' ', @mapped_name, 'Exact')"/>
	</xsl:variable> 
		public <xsl:value-of select="$signature"/>
		{
			get
			{
				return (<xsl:value-of select="@mapped_name"/>.HasValue) ? <xsl:value-of select="@mapped_name"/>.Value : default(<xsl:value-of select="$type"/>);
			}
		}
</xsl:template>

<xsl:template match="attribute" xml:space="preserve">
            _<xsl:value-of select="@mapped_name"/> = InternalDataContext.ReplacePlaceholders(_<xsl:value-of select="@mapped_name"/>);</xsl:template>    

<xsl:template match="content" mode="Filters">
	<xsl:variable name="name" select="@mapped_name" />
	<xsl:variable name="context_name" select="parent::node()/@class" />
	
	public static IEnumerable&lt;<xsl:value-of select="$name" />&gt; ApplyContext(this IEnumerable&lt;<xsl:value-of select="$name" />&gt; e, <xsl:value-of select="$context_name" /> context)
	{
		foreach(<xsl:value-of select="$name" /> item in e)
			item.InternalDataContext = context;
		return e;   
	}
	
	public static IQueryable&lt;<xsl:value-of select="$name" />&gt; Published(this IQueryable&lt;<xsl:value-of select="$name" />&gt; e)
	{
		return e.Where(n =&gt; n.StatusType.Name == "Published");   
	}
	<xsl:choose>
		<xsl:when test="@use_default_filtration = 'true'">
	public static IQueryable&lt;<xsl:value-of select="$name" />&gt; ForFrontEnd(this IQueryable&lt;<xsl:value-of select="$name" />&gt; e)
	{
		return e;
	}		
		</xsl:when>
		<xsl:otherwise>
	public static IQueryable&lt;<xsl:value-of select="$name" />&gt; ForFrontEnd(this IQueryable&lt;<xsl:value-of select="$name" />&gt; e)
	{
		IQueryable&lt;<xsl:value-of select="$name" />&gt; result = e.Where(n =&gt; n.Visible &amp;&amp; !n.Archive);
		<xsl:choose>
        <xsl:when test="parent::node()/@forStage = 'true'">return result;</xsl:when>
        <xsl:otherwise>return result.Published();</xsl:otherwise>        
		</xsl:choose>
	}		
		</xsl:otherwise>
	</xsl:choose>
	


</xsl:template>

<xsl:template name="ListSelector">
public class BindingListSelector&lt;TSource, T&gt; : ListSelector&lt;TSource, T&gt;, IBindingList
{
	public BindingListSelector(IBindingList source, Func&lt;TSource, T&gt; selector, Action&lt;IList&lt;TSource&gt;, T&gt; onAdd, Action&lt;IList&lt;TSource&gt;, T&gt; onRemove):base(source as IList&lt;TSource&gt;, selector, onAdd, onRemove)
	{
		sourceAsBindingList = source;
	}
	
	protected IBindingList sourceAsBindingList;
	
	#region IBindingList Members
	
	public void AddIndex(PropertyDescriptor property)
	{
		sourceAsBindingList.AddIndex(property);
	}
	
	public object AddNew()
	{
		return sourceAsBindingList.AddNew();
	}
	
	public bool AllowEdit
	{
		get { return sourceAsBindingList.AllowEdit; }
	}
	
	public bool AllowNew
	{
		get { return sourceAsBindingList.AllowNew; }
	}
	
	public bool AllowRemove
	{
		get { return sourceAsBindingList.AllowRemove; }
	}
	
	public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
	{
		sourceAsBindingList.ApplySort(property, direction);
	}
	
	public int Find(PropertyDescriptor property, object key)
	{
		return sourceAsBindingList.Find(property, key);
	}
	
	public bool IsSorted
	{
		get { return sourceAsBindingList.IsSorted; }
	}
	
	public event ListChangedEventHandler ListChanged
	{
		add
		{
			sourceAsBindingList.ListChanged += value;
		}
		remove
		{
			sourceAsBindingList.ListChanged -= value;
		}
	}
	
	public void RemoveIndex(PropertyDescriptor property)
	{
		sourceAsBindingList.RemoveIndex(property);
	}
	
	public void RemoveSort()
	{
		sourceAsBindingList.RemoveSort();
	}
	
	public ListSortDirection SortDirection
	{
		get { return sourceAsBindingList.SortDirection; }
	}
	
	public PropertyDescriptor SortProperty
	{
		get { return sourceAsBindingList.SortProperty; }
	}
	
	public bool SupportsChangeNotification
	{
		get { return sourceAsBindingList.SupportsChangeNotification; }
	}
	
	public bool SupportsSearching
	{
		get { return sourceAsBindingList.SupportsSearching; }
	}
	
	public bool SupportsSorting
	{
		get { return sourceAsBindingList.SupportsSorting; }
	}
	
	#endregion
}
	
public static class ListSelectorExtensions
{
	public static ListSelector&lt;TSource, T&gt; AsListSelector&lt;TSource, T&gt;(this IList&lt;TSource&gt; source, Func&lt;TSource, T&gt; selector, Action&lt;IList&lt;TSource&gt;, T&gt; onAdd, Action&lt;IList&lt;TSource&gt;, T&gt; onRemove)
	{
		return new ListSelector&lt;TSource, T&gt;(source, selector, onAdd, onRemove);
	}
	public static BindingListSelector&lt;TSource, T&gt; AsListSelector&lt;TSource, T&gt;(this IBindingList source, Func&lt;TSource, T&gt; selector, Action&lt;IList&lt;TSource&gt;, T&gt; onAdd, Action&lt;IList&lt;TSource&gt;, T&gt; onRemove)
	{
		return new BindingListSelector&lt;TSource, T&gt;(source, selector, onAdd, onRemove);
	}
}
	
public class ListSelector&lt;TSource, T&gt; : IList&lt;T&gt;, IList
{
	public ListSelector(IList&lt;TSource&gt; source, Func&lt;TSource, T&gt; selector, Action&lt;IList&lt;TSource&gt;, T&gt; onAdd, Action&lt;IList&lt;TSource&gt;, T&gt; onRemove)
	{
		this.source = source;
		this.selector = selector;
		this.onAdd = onAdd;
		this.onRemove = onRemove;
		UpdateProjection();
	}
	
	protected IList&lt;TSource&gt; source;
	protected Func&lt;TSource, T&gt; selector;
	protected List&lt;T&gt; projection;
	protected Action&lt;IList&lt;TSource&gt;, T&gt; onAdd;
	protected Action&lt;IList&lt;TSource&gt;, T&gt; onRemove;
	
	#region IList&lt;T&gt; Members
	
	public int IndexOf(T item)
	{
		int i = 0;
		foreach (T t in projection)
		{
			if (t.Equals(item))
			return i;
			i++;
		}
		return -1;
	}
	
	private void UpdateProjection()
	{
		projection = source.Select(selector).Where(n => n != null).ToList();		
	}
	
	public void Insert(int index, T item)
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	public void RemoveAt(int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	public T this[int index]
	{
		get
		{
			return projection[index];
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
	
	#endregion
	
	#region ICollection&lt;T&gt; Members
	
	public void Add(T item)
	{
		if (onAdd != null) {
			onAdd(source, item);
			UpdateProjection();
		}
	}
	
	public void Add(IEnumerable&lt;T&gt; items)
	{
		if (items != null) {
			foreach (T item in items)
			{
				Add(item);
			}
		}
	}
	
	public void Remove(IEnumerable&lt;T&gt; items)
	{
		if (items != null) {
			foreach (T item in items.ToList())
			{
				Remove(item);
			}
		}
	}
	
	public bool Remove(T item)
	{
		if (onRemove != null)
		{
			onRemove(source, item);
			UpdateProjection();
			return true;
		}
		else
			return false;
	}
	
	public void Clear()
	{
		foreach (T item in projection.ToList())
		{
			Remove(item);
		}
	}
	
	public bool Contains(T item)
	{
		return projection.Contains(item);
	}
	
	public void CopyTo(T[] array, int arrayIndex)
	{
		projection.CopyTo(array, arrayIndex);
	}
	
	public int Count
	{
		get { return projection.Count(); }
	}
	
	public bool IsReadOnly
	{
		get { return true; }
	}
	
	#endregion
	
	#region IEnumerable&lt;T&gt; Members
	
	public IEnumerator&lt;T&gt; GetEnumerator()
	{
		return projection.GetEnumerator();
	}
	
	#endregion
	
	#region IEnumerable Members
	
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return projection.GetEnumerator();
	}
	
	#endregion
	
	#region IList Members
	
	int IList.Add(object value)
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	void IList.Clear()
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	bool IList.Contains(object value)
	{
		return Contains((T) value);
	}
	
	int IList.IndexOf(object value)
	{
		return IndexOf((T) value);
	}
	
	void IList.Insert(int index, object value)
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	bool IList.IsFixedSize
	{
		get { return true; }
	}
	
	bool IList.IsReadOnly
	{
		get { return true; }
	}
	
	void IList.Remove(object value)
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	void IList.RemoveAt(int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}
	
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			
		}
	}
	
	#endregion
	
	#region ICollection Members
	
	void ICollection.CopyTo(Array array, int index)
	{
		T[] arrayOfT = array as T[];
		if (arrayOfT == null)
			throw new ArgumentException("Incorrect array type");
		this.CopyTo(arrayOfT, index);
	}
	
	int ICollection.Count
	{
		get { return this.Count; }
	}
	
	bool ICollection.IsSynchronized
	{
		get { return false; }
	}
	
	object ICollection.SyncRoot
	{
		get { throw new Exception("The method or operation is not implemented."); }
	}
	
	#endregion
}
    </xsl:template>

<xsl:template name="GetType">
	<xsl:param name="type" />
	<xsl:param name="size" />
	<xsl:param name="is_long" />

	<xsl:variable name="end_size">
		<xsl:choose>
			<xsl:when test="$size">
				<xsl:value-of select="$size"></xsl:value-of>
			</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:choose>
		<xsl:when test="$type='String' or $type='Textbox' or $type='VisualEdit' or $type='File' or $type='Image' or $type='Dynamic Image'">System.String</xsl:when>
		<xsl:when test="$type='Date' or $type='Time' or $type='DateTime'">System.DateTime</xsl:when>
		<xsl:when test="$type='O2M'">System.Int32</xsl:when>
		<xsl:when test="$type='Numeric' and $end_size = '0' and $is_long = 'true'">System.Int64</xsl:when>
		<xsl:when test="$type='Numeric' and $end_size = '0'">System.Int32</xsl:when>
		<xsl:when test="$type='Numeric' and $end_size != '0' and $is_long = 'true'">System.Decimal</xsl:when>		
		<xsl:when test="$type='Numeric' and $end_size != '0'">System.Double</xsl:when>
		<xsl:when test="$type='Boolean'">System.Boolean</xsl:when>
	</xsl:choose>
</xsl:template>

<xsl:template match="attribute" mode="DetachImplicitBackAssociation">
	<xsl:variable name="content_plural_name">
		<xsl:value-of select="parent::node()/@plural_mapped_name" />
	</xsl:variable>
	<xsl:if test="$content_plural_name != ''">
		<xsl:variable name="back_name">
			<xsl:choose>
				<xsl:when test="@mapped_back_name">
					<xsl:value-of select="@mapped_back_name"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$content_plural_name" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
			this._<xsl:value-of select="$back_name" /> = Detach(this._<xsl:value-of select="$back_name" />, attach_<xsl:value-of select="$back_name" />, detach_<xsl:value-of select="$back_name" />);
	</xsl:if>		
</xsl:template>
	
<xsl:template match="attribute" mode="DetachExplicitBackAssociation">
	<xsl:variable name="related_content_id">
		<xsl:value-of select="@related_content_id" />
	</xsl:variable>
	<xsl:variable name="related_mapped_name">
		<xsl:value-of select="parent::node()/parent::node()/content[@id=$related_content_id]/@mapped_name" />
	</xsl:variable>
	<xsl:if test="$related_mapped_name != ''">
			this._<xsl:value-of select="@mapped_name" /> = Detach(this._<xsl:value-of select="@mapped_name" />, attach_<xsl:value-of select="@mapped_name" />, detach_<xsl:value-of select="@mapped_name" />);
	</xsl:if>
</xsl:template>
	
<xsl:template match="attribute" mode="DetachAssociation">
	<xsl:variable name="related_content_id">
		<xsl:value-of select="@related_content_id" />
	</xsl:variable>
	<xsl:variable name="related_mapped_name">
		<xsl:value-of select="parent::node()/parent::node()/content[@id=$related_content_id]/@mapped_name" />
	</xsl:variable>
	<xsl:if test="$related_mapped_name != ''">
			this._<xsl:value-of select="@mapped_name" />1 = Detach(this._<xsl:value-of select="@mapped_name" />1);
	</xsl:if>			
</xsl:template>

<xsl:template match="attribute" mode="DetachLink">
	<xsl:variable name="link_id" select="@link_id" />
	<xsl:apply-templates select="parent::node()/parent::node()/link[@id=$link_id]" mode="DetachLink" />
</xsl:template>

<xsl:template match="link" mode="DetachLink">
	<xsl:if test="@plural_mapped_name != ''">
			this._<xsl:value-of select="@plural_mapped_name" /> = Detach(this._<xsl:value-of select="@plural_mapped_name" />, attach_<xsl:value-of select="@plural_mapped_name" />, detach_<xsl:value-of select="@plural_mapped_name" />);
	</xsl:if>		
</xsl:template>
</xsl:stylesheet>
