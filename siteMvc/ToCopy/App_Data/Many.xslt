<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">

    <xsl:output method="text" />
    
<xsl:template match="schema">
using System.Data.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections;
using System.ComponentModel;

    <xsl:if test="@namespace">
namespace <xsl:value-of select="@namespace" /> 
{
</xsl:if>

<xsl:apply-templates select="content/attribute[@type = 'M2M']/parent::node()" />
<xsl:apply-templates select="link" />

<xsl:if test="@namespace">
}
</xsl:if>

</xsl:template>



<xsl:template match="content">
public partial class <xsl:value-of select="@mapped_name" />
{
    <xsl:apply-templates select="attribute[@type = 'M2M']">
        <xsl:with-param name="virtual" select="@virtual" />
    </xsl:apply-templates>
}
</xsl:template>
 
<xsl:template match="attribute">
    <xsl:param name="virtual" />
      <xsl:variable name="related_content_id"><xsl:value-of select="@related_content_id" /></xsl:variable>
      <xsl:variable name="content_id"><xsl:value-of select="parent::node()/@id" /></xsl:variable>
    <xsl:variable name="link_id"><xsl:value-of select="@link_id" /></xsl:variable>
    <xsl:variable name="related_member_name" select="parent::node()/parent::node()/content[@id=$related_content_id]/@plural_mapped_name" />
    <xsl:variable name="related_class_name" select="parent::node()/parent::node()/content[@id=$related_content_id]/@mapped_name" />
    <xsl:variable name="link_member_name" select="parent::node()/parent::node()/link[@id=$link_id]/@plural_mapped_name" />
  <xsl:variable name="linked_member_name">
    <xsl:choose>
        <xsl:when test="$related_content_id = $content_id"><xsl:value-of select="concat($related_class_name, '2')" /></xsl:when>
        <xsl:otherwise><xsl:value-of select="$related_class_name" /></xsl:otherwise>
    </xsl:choose> 
  </xsl:variable>
  <xsl:variable name="linked_id_member_name">
    <xsl:choose>
        <xsl:when test="$related_content_id = $content_id"><xsl:value-of select="concat($related_class_name, '_ID', '2')" /></xsl:when>
        <xsl:otherwise><xsl:value-of select="concat($related_class_name, '_ID')" /></xsl:otherwise>
    </xsl:choose> 
  </xsl:variable>
    <xsl:variable name="link_class_name" select="parent::node()/parent::node()/link[@id=$link_id]/@mapped_name" />
  <xsl:variable name="dbIndependent" select="parent::node()/parent::node()/@dbIndependent" />
  <xsl:variable name="out_link_id">
    <xsl:choose>
        <xsl:when test="$dbIndependent = 'true'">InternalDataContext.Cnn.GetLinkIdByNetName(InternalDataContext.SiteId, "<xsl:value-of select="$link_class_name" />")</xsl:when>
        <xsl:otherwise><xsl:value-of select="$link_id" /></xsl:otherwise>
    </xsl:choose> 
  </xsl:variable>
    <xsl:variable name="class_name" select="parent::node()/@mapped_name" />
    <xsl:if test="$related_class_name != ''">
        <xsl:choose>
            <xsl:when test="$virtual = '0' and $related_class_name != ''" >

    private ListSelector&lt;<xsl:value-of select="$link_class_name" />, <xsl:value-of select="$related_class_name" />&gt; _<xsl:value-of select="@mapped_name" /> = null;

    public ListSelector&lt;<xsl:value-of select="$link_class_name" />, <xsl:value-of select="$related_class_name" />&gt; <xsl:value-of select="@mapped_name" />
    {
        get
        {
            if (_<xsl:value-of select="@mapped_name" /> == null)
            {
                var result = this.<xsl:value-of select="$link_member_name" />.GetNewBindingList()
                .AsListSelector&lt;<xsl:value-of select="$link_class_name" />, <xsl:value-of select="$related_class_name" />&gt;
                (
                    od =&gt; od.<xsl:value-of select="$linked_member_name" />,
                    delegate(IList&lt;<xsl:value-of select="$link_class_name" />&gt; ods, <xsl:value-of select="$related_class_name" /> p)
                    {
                        var items = ods.Where(od =&gt; od.<xsl:value-of select="$linked_id_member_name" /> == p.Id);
                        if (!items.Any())
                        {
                            this.Modified = System.DateTime.Now;
                            var item = new <xsl:value-of select="$link_class_name" />();
                            item.<xsl:value-of select="$class_name" />_ID = this.Id;
                            item.<xsl:value-of select="$linked_member_name" /> = p;
                            item.InsertWithArticle = true;
                            ods.Add(item);
                        }
                    },
                    delegate(IList&lt;<xsl:value-of select="$link_class_name" />&gt; ods, <xsl:value-of select="$related_class_name" /> p)
                    {
                        var items = ods.Where(od =&gt; od.<xsl:value-of select="$linked_id_member_name" /> == p.Id);
                        if (items.Any())
                        {
                            this.Modified = System.DateTime.Now;
                            var item = items.Single();
                            InternalDataContext.<xsl:value-of select="$link_member_name" />.DeleteOnSubmit(item);							
                            item.RemoveWithArticle = true;
                            ods.Remove(item);
                        }
                    }
                );
                if (this.PropertyChanging == null)
                    return result;
                else
                    _<xsl:value-of select="@mapped_name" /> = result;				
            }
            
            return _<xsl:value-of select="@mapped_name" />;
        }
    }
            </xsl:when>
      <xsl:otherwise>
    public IQueryable&lt;<xsl:value-of select="$related_class_name" />&gt; <xsl:value-of select="@mapped_name" />
    {
        get
        {
            int linkId = <xsl:value-of select="$out_link_id" />;
            <xsl:if test="$dbIndependent = 'true'" >
            if (linkId == 0)
                throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "<xsl:value-of select="$link_class_name" />", InternalDataContext.SiteId));
            </xsl:if>	
            return InternalDataContext.ExecuteQuery&lt;<xsl:value-of select="$related_class_name" />&gt;(String.Format("EXEC sp_executesql N'select c.* from content_{0} c inner join item_link i on c.content_item_id = i.linked_item_id where i.item_id = @itemId and i.link_id = @linkId', N'@linkId NUMERIC, @itemId NUMERIC', @linkId = {2}, @itemId = {1}", <xsl:value-of select="$related_content_id" />, Id, linkId)).AsQueryable();
        }
    }
            </xsl:otherwise>
    </xsl:choose>
    public string[] <xsl:value-of select="@mapped_name" />IDs
    {
        get
        {
            return <xsl:value-of select="@mapped_name" />.Select(n => n.Id.ToString()).ToArray();
        }
    }
    
    public string <xsl:value-of select="@mapped_name" />String
    {
        get
        {
            return String.Join(",", <xsl:value-of select="@mapped_name" />IDs.ToArray());
        }
    }
    </xsl:if>
</xsl:template>

<xsl:template match="link">
    <xsl:variable name="name" select="@mapped_name" />
    <xsl:variable name="link_id">
        <xsl:choose>
            <xsl:when test="parent::node()/@dbIndependent = 'true'">InternalDataContext.Cnn.GetLinkIdByNetName(InternalDataContext.SiteId, "<xsl:value-of select="$name" />")</xsl:when>
            <xsl:otherwise><xsl:value-of select="@id" /></xsl:otherwise>
        </xsl:choose> 
    </xsl:variable>
    
public partial class <xsl:value-of select="$name" /> : QPLinkBase, IQPLink
{
    public override int Id
    {
        get
        {
            return _ITEM_ID;
        }
    }
    
    public override int LinkedItemId
    {
        get
        {
            return _LINKED_ITEM_ID;
        }
    }
    
        
    [Obsolete]
    public decimal ITEM_ID
    {
        get
        {
            return _ITEM_ID;
        }
    }
    
    [Obsolete]
    public decimal LINKED_ITEM_ID
    {
        get
        {
            return _LINKED_ITEM_ID;
        }
    }
    
    public override void Detach()
    {
        if (null == PropertyChanging)
            return;

        PropertyChanging = null;
        PropertyChanged = null;
            
        <xsl:variable name="suffix" ></xsl:variable>

        <xsl:variable name="suffix2" >
            <xsl:if test="@content_id = @linked_content_id">
                <xsl:value-of select="2" />
            </xsl:if>
        </xsl:variable>
        
        <xsl:apply-templates select="." mode="DetachJunctionLink">
            <xsl:with-param name="suffix" select="$suffix" />
            <xsl:with-param name="content_id" select ="@content_id" />
        </xsl:apply-templates>
    
        <xsl:apply-templates select="." mode="DetachJunctionLink" >
            <xsl:with-param name="suffix" select="$suffix2" />
            <xsl:with-param name="content_id" select ="@linked_content_id" />
        </xsl:apply-templates>
    }

    public override int LinkId
    {
        get
        {
            int linkId = <xsl:value-of select="$link_id" />;
            <xsl:if test="parent::node()/@dbIndependent = 'true'" >
            if (linkId == 0)
                throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "<xsl:value-of select="$name" />", InternalDataContext.SiteId));
            </xsl:if>
            return linkId;
        }
    }
}
</xsl:template>

<xsl:template match="link" mode="DetachJunctionLink">
    <xsl:param name="suffix" />
    <xsl:param name="content_id" />
    <xsl:variable name="mapped_content_name">
        <xsl:value-of select="parent::node()/content[@id=$content_id]/@mapped_name" />
    </xsl:variable>
    <xsl:variable name="storage">
        <xsl:value-of select="concat('_', $mapped_content_name, '1', $suffix)"/>
    </xsl:variable>
        this.<xsl:value-of select="$storage" /> = Detach(this.<xsl:value-of select="$storage" />);
</xsl:template>

</xsl:stylesheet>
