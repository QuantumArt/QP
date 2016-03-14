<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="xml" indent="yes"/>

	<xsl:template match="schema">
		<Database xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
			<xsl:attribute name="Name">
				<xsl:value-of select="@customer_code"/>
			</xsl:attribute>
			<xsl:attribute name="Class">
				<xsl:value-of select="@class"/>
			</xsl:attribute>
			<Connection Mode="WebSettings">
				<xsl:attribute name="ConnectionString">
					<xsl:value-of select="@connectionString"/>
				</xsl:attribute>
				<xsl:attribute name="SettingsObjectName">
					<xsl:value-of select="@connectionStringObject"/>
				</xsl:attribute>
				<xsl:attribute name="SettingsPropertyName">
					<xsl:value-of select="@connectionStringName"/>
				</xsl:attribute>
				<xsl:attribute name="Provider">System.Data.SqlClient</xsl:attribute>
			</Connection>
			<xsl:call-template name="status_type" />
			<xsl:apply-templates select="content"/>
			<xsl:apply-templates select="link"/>
			<Function Name="dbo.qp_abs_time" Method="AbsTime" IsComposable="true">
				<Parameter Name="time" Type="System.DateTime" DbType="DateTime" />
				<Return Type="System.Int32" DbType="Decimal" />
			</Function>
		</Database>
	</xsl:template>

	<xsl:template name="status_type">

		<Table xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007" Name="STATUS_TYPE" Member="StatusTypes">
			<Type Name="StatusType">
				<Column Name="SITE_ID" Member="SiteId" Type="System.Int32" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" />
				<Column Name="STATUS_TYPE_ID" Member="Id" Type="System.Int32" DbType="Decimal(18,0) NOT NULL IDENTITY" IsPrimaryKey="true" IsDbGenerated="true" CanBeNull="false" />
				<Column Name="STATUS_TYPE_NAME" Member="Name" Type="System.String" DbType="NVarChar(255) NOT NULL" CanBeNull="false" />
				<Column Name="WEIGHT" Member="Weight" Type="System.Int32" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" />
				<Column Name="DESCRIPTION" Member="Description" Type="System.String" DbType="NVarChar(512)" CanBeNull="true" />
				<Column Name="CREATED" Member="Created" Type="System.DateTime" DbType="DateTime NOT NULL" CanBeNull="false" />
				<Column Name="MODIFIED" Member="Modified" Type="System.DateTime" DbType="DateTime NOT NULL" CanBeNull="false" />
				<Column Name="LAST_MODIFIED_BY" Member="LastModifiedBy" Type="System.Int32" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" />
			</Type>
		</Table>
	</xsl:template>


	<xsl:template match="content">
		<Table xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
			<xsl:attribute name="Name">
				<xsl:choose>
					<xsl:when test="@use_default_filtration = 'false'">
						<xsl:value-of select="concat('dbo.CONTENT_', @id)"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="parent::node()/@forStage = 'true'">
								<xsl:value-of select="concat('dbo.CONTENT_', @id, '_STAGE')"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="concat('dbo.CONTENT_', @id, '_LIVE')"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:attribute name="Member">
				<xsl:value-of select="@plural_mapped_name"/>
			</xsl:attribute>
			<Type>
				<xsl:variable name="name">
					<xsl:value-of select="@mapped_name" />
				</xsl:variable>
				<xsl:variable name="id">
					<xsl:value-of select="@id" />
				</xsl:variable>
				<xsl:attribute name="Name">
					<xsl:value-of select="$name"/>
				</xsl:attribute>
				<Column Name="CONTENT_ITEM_ID" Type="System.Int32" Member="Id" DbType="Decimal(18,0) IDENTITY NOT NULL" IsPrimaryKey="true" CanBeNull="false" IsDbGenerated="true" />
				<Column Name="STATUS_TYPE_ID" Type="System.Int32" Member="StatusTypeId" DbType="Decimal(18,0) NOT NULL" CanBeNull="false"  />
				<Column Name="VISIBLE" Type="System.Boolean" Member="Visible" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" />
				<Column Name="ARCHIVE" Type="System.Boolean" Member="Archive" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" />
				<Column Name="CREATED" Type="System.DateTime" Member="Created" DbType="DateTime NOT NULL" CanBeNull="false" />
				<Column Name="MODIFIED" Type="System.DateTime" Member="Modified" DbType="DateTime NOT NULL" CanBeNull="false" />
				<Column Name="LAST_MODIFIED_BY" Type="System.Int32" Member="LastModifiedBy" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" />

				<xsl:apply-templates select="attribute[@type!='M2M' and @type!='M2O']" mode="Column"/>

				<xsl:apply-templates select="attribute[@type='O2M']" mode="Association">
					<xsl:with-param name="content_name" select="$name" />
				</xsl:apply-templates>

				<xsl:apply-templates select="attribute[@type='M2O']" mode="ExplicitBackAssociation">
					<xsl:with-param name="content_name" select="$name" />
				</xsl:apply-templates>

				<xsl:if test="@virtual = '0'">
					<xsl:apply-templates select="attribute[@type='M2M']" mode="Link"></xsl:apply-templates>
				</xsl:if>

				<xsl:apply-templates select="parent::node()/content/attribute[@type='O2M' and @has_m2o='false' and @related_content_id=$id]" mode="ImplicitBackAssociation">
				</xsl:apply-templates>

				<Association Name="StatusType_ContentItem" Member="StatusType" ThisKey="StatusTypeId" OtherKey="Id" Type="StatusType" IsForeignKey="true" />

			</Type>
		</Table>

	</xsl:template>

	<xsl:template match="link">
		<Table xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
			<xsl:attribute name="Name">
				<xsl:choose>
					<xsl:when test="parent::node()/@forStage = 'true'">
						<xsl:value-of select="concat('dbo.LINK_', @id, '_UNITED')"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat('dbo.LINK_', @id)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:attribute name="Member">
				<xsl:value-of select="@plural_mapped_name"/>
			</xsl:attribute>
			<Type>
				<xsl:variable name="content_id">
					<xsl:value-of select="@content_id" />
				</xsl:variable>
				<xsl:variable name="linked_content_id">
					<xsl:value-of select="@linked_content_id" />
				</xsl:variable>
				<xsl:variable name="link_id">
					<xsl:value-of select="@id" />
				</xsl:variable>
				<xsl:variable name="mapped_content_name">
					<xsl:value-of select="parent::node()/content[@id=$content_id]/@mapped_name" />
				</xsl:variable>
				<xsl:variable name="mapped_linked_content_name">
					<xsl:value-of select="parent::node()/content[@id=$linked_content_id]/@mapped_name" />
				</xsl:variable>


				<xsl:attribute name="Name">
					<xsl:value-of select="@mapped_name"/>
				</xsl:attribute>

				<xsl:variable name="suffix" ></xsl:variable>

				<xsl:variable name="suffix2" >
					<xsl:if test="$content_id = $linked_content_id">
						<xsl:value-of select="2" />
					</xsl:if>
				</xsl:variable>



				<Column Name="ITEM_ID" Type="System.Int32" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" IsPrimaryKey="true">
					<xsl:attribute name="Member">
						<xsl:value-of select="concat($mapped_content_name, '_ID', $suffix)"/>
					</xsl:attribute>
					<xsl:attribute name="Storage">_ITEM_ID</xsl:attribute>

				</Column>
				<Column Name="LINKED_ITEM_ID" Type="System.Int32" DbType="Decimal(18,0) NOT NULL" CanBeNull="false" IsPrimaryKey="true">
					<xsl:attribute name="Member">
						<xsl:value-of select="concat($mapped_linked_content_name, '_ID', $suffix2)"/>
					</xsl:attribute>
					<xsl:attribute name="Storage">_LINKED_ITEM_ID</xsl:attribute>
				</Column>

				<xsl:apply-templates select="." mode="JunctionLink">
					<xsl:with-param name="suffix" select="$suffix" />
					<xsl:with-param name="content_id" select ="$content_id" />
				</xsl:apply-templates>
				<xsl:apply-templates select="." mode="JunctionLink" >
					<xsl:with-param name="suffix" select="$suffix2" />
					<xsl:with-param name="content_id" select ="$linked_content_id" />
				</xsl:apply-templates>

			</Type>
		</Table>

	</xsl:template>

	<xsl:template match="link" mode="JunctionLink">
		<xsl:param name="suffix" />
		<xsl:param name="content_id" />

		<xsl:variable name="mapped_content_name">
			<xsl:value-of select="parent::node()/content[@id=$content_id]/@mapped_name" />
		</xsl:variable>
		<xsl:if test="$mapped_content_name != ''">
			<Association xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
				<xsl:attribute name="Name">
					<xsl:value-of select="concat($mapped_content_name, '_', @id)" />
				</xsl:attribute>
				<xsl:attribute name="Member">
					<xsl:value-of select="concat($mapped_content_name, $suffix)" />
				</xsl:attribute>
				<xsl:attribute name="Storage">
					<xsl:value-of select="concat('_', $mapped_content_name, '1', $suffix)"/>
				</xsl:attribute>
				<xsl:attribute name="ThisKey">
					<xsl:value-of select="concat($mapped_content_name, '_ID', $suffix)" />
				</xsl:attribute>
				<xsl:attribute name="OtherKey">Id</xsl:attribute>
				<xsl:attribute name="Type">
					<xsl:value-of select="$mapped_content_name"/>
				</xsl:attribute>
				<xsl:attribute name="IsForeignKey">true</xsl:attribute>
			</Association>
		</xsl:if>
	</xsl:template>

	<xsl:template match="attribute" mode="Link">
		<xsl:variable name="link_id" select="@link_id" />
		<xsl:apply-templates select="parent::node()/parent::node()/link[@id=$link_id]" mode="Association">
			<xsl:with-param name="mapped_content_name" select="parent::node()/@mapped_name" />
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="link" mode="Association">
		<xsl:param name="mapped_content_name" />
		<xsl:if test="@mapped_name != ''">
			<Association xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
				<xsl:attribute name="Name">
					<xsl:value-of select="concat($mapped_content_name, '_', @id)" />
				</xsl:attribute>
				<xsl:attribute name="Member">
					<xsl:value-of select="@plural_mapped_name" />
				</xsl:attribute>
				<xsl:attribute name="ThisKey">Id</xsl:attribute>
				<xsl:attribute name="OtherKey">
					<xsl:value-of select="concat($mapped_content_name, '_ID')"/>
				</xsl:attribute>
				<xsl:attribute name="Type">
					<xsl:value-of select="@mapped_name"/>
				</xsl:attribute>
			</Association>
		</xsl:if>
	</xsl:template>


	<xsl:template match="attribute" mode="Column">
		<Column xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
			<xsl:attribute name="Name">
				<xsl:value-of select="concat('[', @name, ']')" />
			</xsl:attribute>
			<xsl:attribute name="Member">
				<xsl:value-of select="@name" />
			</xsl:attribute>
			<xsl:choose>
				<xsl:when test="@type='O2M'">
					<xsl:attribute name="Member">
						<xsl:value-of select="concat(@mapped_name,'_ID')" />
					</xsl:attribute>
					<xsl:attribute name="Storage">
						<xsl:value-of select="concat('_',@mapped_name)" />
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@mapped_name">
						<xsl:attribute name="Member">
							<xsl:value-of select="@mapped_name" />
						</xsl:attribute>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>

			<xsl:variable name="type">
				<xsl:call-template name="GetType">
					<xsl:with-param name="type" select="@type" />
					<xsl:with-param name="size" select="@size" />
					<xsl:with-param name="is_long" select="@is_long" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:attribute name="Type">
				<xsl:value-of select="$type" />
			</xsl:attribute>

			<xsl:variable name="dbtype">
				<xsl:call-template name="GetDbType">
					<xsl:with-param name="type" select="@type" />
					<xsl:with-param name="size" select="@size" />
          <xsl:with-param name="force_db_type" select="@force_db_type" />
        </xsl:call-template>
			</xsl:variable>
			<xsl:attribute name="DbType">
				<xsl:value-of select="$dbtype" />
			</xsl:attribute>



			<xsl:attribute name="CanBeNull">true</xsl:attribute>
			<xsl:if test="@type='Textbox' or @type='VisualEdit'">
				<xsl:attribute name="UpdateCheck">Never</xsl:attribute>
			</xsl:if>

		</Column>
	</xsl:template>


	<xsl:template match="attribute" mode="Association">
		<xsl:param name="content_name" />
		<xsl:variable name="related_content_id">
			<xsl:value-of select="@related_content_id" />
		</xsl:variable>
		<xsl:variable name="related_type">
			<xsl:value-of select="parent::node()/parent::node()/content[@id=$related_content_id]/@mapped_name" />
		</xsl:variable>
		<xsl:if test="$related_type != ''">
			<Association xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
				<xsl:attribute name="Name">
					<xsl:value-of select="concat(@mapped_name, '_', $content_name)" />
				</xsl:attribute>
				<xsl:attribute name="Member">
					<xsl:value-of select="@mapped_name" />
				</xsl:attribute>
				<xsl:attribute name="Storage">
					<xsl:value-of select="concat('_', @mapped_name, '1')"/>
				</xsl:attribute>
				<xsl:attribute name="ThisKey">
					<xsl:value-of select="concat(@mapped_name, '_ID')"/>
				</xsl:attribute>
				<xsl:attribute name="OtherKey">Id</xsl:attribute>
				<xsl:attribute name="Type">
					<xsl:value-of select="$related_type"/>
				</xsl:attribute>
				<xsl:attribute name="IsForeignKey">true</xsl:attribute>
			</Association>
		</xsl:if>
	</xsl:template>

	<xsl:template match="attribute" mode="ImplicitBackAssociation">
		<xsl:variable name="content_name" select="parent::node()/@mapped_name"/>
		<xsl:variable name="plural_content_name" select="parent::node()/@plural_mapped_name"/>
		<xsl:variable name="back_name">
			<xsl:choose>
				<xsl:when test="@mapped_back_name">
					<xsl:value-of select="@mapped_back_name"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="parent::node()/@plural_mapped_name" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:if test="$content_name != ''">
			<Association xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
				<xsl:attribute name="Name">
					<xsl:value-of select="concat(@mapped_name, '_', $content_name)" />
				</xsl:attribute>
				<xsl:attribute name="Member">
					<xsl:value-of select="$back_name" />
				</xsl:attribute>
				<xsl:attribute name="ThisKey">Id</xsl:attribute>
				<xsl:attribute name="OtherKey">
					<xsl:value-of select="concat(@mapped_name, '_ID')"/>
				</xsl:attribute>
				<xsl:attribute name="Type">
					<xsl:value-of select="$content_name"/>
				</xsl:attribute>
			</Association>
		</xsl:if>
	</xsl:template>

	<xsl:template match="attribute" mode="ExplicitBackAssociation">
		<xsl:param name="content_name" />
		<xsl:variable name="related_content_id">
			<xsl:value-of select="@related_content_id" />
		</xsl:variable>
		<xsl:variable name="related_attribute_id">
			<xsl:value-of select="@related_attribute_id" />
		</xsl:variable>
		<xsl:variable name="related_content_name">
			<xsl:value-of select="parent::node()/parent::node()/content[@id=$related_content_id]/@mapped_name" />
		</xsl:variable>
		<xsl:variable name="related_attribute_name">
			<xsl:value-of select="parent::node()/parent::node()/content[@id=$related_content_id]/attribute[@id=$related_attribute_id]/@mapped_name" />
		</xsl:variable>
		<xsl:if test="$related_content_name != ''">
			<Association xmlns="http://schemas.microsoft.com/linqtosql/dbml/2007">
				<xsl:attribute name="Name">
					<xsl:value-of select="concat($related_attribute_name, '_', $related_content_name)" />
				</xsl:attribute>
				<xsl:attribute name="Member">
					<xsl:value-of select="@mapped_name" />
				</xsl:attribute>
				<xsl:attribute name="ThisKey">Id</xsl:attribute>
				<xsl:attribute name="OtherKey">
					<xsl:value-of select="concat($related_attribute_name, '_ID')"/>
				</xsl:attribute>
				<xsl:attribute name="Type">
					<xsl:value-of select="$related_content_name"/>
				</xsl:attribute>
			</Association>
		</xsl:if>
	</xsl:template>


	<xsl:template name="GetDbType">
		<xsl:param name="type" />
		<xsl:param name="size" />
    <xsl:param name="force_db_type" />    
		<xsl:variable name="end_size">
			<xsl:choose>
				<xsl:when test="$size">
					<xsl:value-of select="$size"></xsl:value-of>
				</xsl:when>
				<xsl:otherwise>255</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
      <xsl:when test="$force_db_type">
        <xsl:value-of select="$force_db_type"/>
      </xsl:when>
			<xsl:when test="$type='String'">
				<xsl:value-of select="concat('NVarChar(', $end_size ,')')"/>
			</xsl:when>
			<xsl:when test="$type='File' or $type='Image' or $type='Dynamic Image'">NVarChar(255)</xsl:when>
			<xsl:when test="$type='Textbox' or $type='VisualEdit'">NText</xsl:when>
			<xsl:when test="$type='Date' or $type='Time' or $type='DateTime'">DateTime</xsl:when>
			<xsl:when test="$type='Numeric'">Decimal(38,0)</xsl:when>
			<xsl:when test="$type='Boolean' or $type='O2M'">Decimal(18,0)</xsl:when>
		</xsl:choose>
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
			<xsl:when test="$type='Numeric' and $end_size='0' and $is_long = 'true'">System.Int64</xsl:when>
			<xsl:when test="$type='Numeric' and $end_size='0'">System.Int32</xsl:when>
			<xsl:when test="$type='Numeric' and $end_size!='0' and $is_long = 'true'">System.Decimal</xsl:when>			
			<xsl:when test="$type='Numeric' and $end_size!='0'">System.Double</xsl:when>
			<xsl:when test="$type='Boolean'">System.Boolean</xsl:when>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
