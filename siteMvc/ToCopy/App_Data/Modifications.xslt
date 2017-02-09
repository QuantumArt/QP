<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">

<xsl:output method="text" />
    
<xsl:template match="schema">
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

<xsl:if test="@namespace">
namespace <xsl:value-of select="@namespace" /> 
{
</xsl:if>

public partial class <xsl:value-of select="@class" />
{


    <xsl:apply-templates select="content" />
    <xsl:apply-templates select="link" />

    partial void InsertStatusType(StatusType instance) 
    {
    }

    partial void UpdateStatusType(StatusType instance)
    {
    }			

    partial void DeleteStatusType(StatusType instance)
    {
    }
}

<xsl:if test="@namespace">
}
</xsl:if>

</xsl:template>

    <xsl:template match="link">
        <xsl:variable name="dbIndependent" select="parent::node()/@dbIndependent" />
        <xsl:variable name="name" select="@mapped_name" />
        <xsl:variable name="link_id">
            <xsl:choose>
                <xsl:when test="$dbIndependent = 'true'">Cnn.GetLinkIdByNetName(SiteId, "<xsl:value-of select="$name" />")</xsl:when>
                <xsl:otherwise><xsl:value-of select="@id" /></xsl:otherwise>
            </xsl:choose> 
        </xsl:variable>

    partial void Insert<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance)
    {
    }

    partial void Update<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance)
    {
    }

    partial void Delete<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance)
    {
    }
    </xsl:template>


<xsl:template match="content">
    <xsl:variable name="name" select="@mapped_name" />
    <xsl:variable name="db_name" select="@name" />
    <xsl:variable name="dbIndependent" select="parent::node()/@dbIndependent" />
    <xsl:variable name="content_id">
        <xsl:choose>
            <xsl:when test="$dbIndependent = 'true'">Cnn.GetContentIdByNetName(SiteId, "<xsl:value-of select="$name" />")</xsl:when>
            <xsl:otherwise><xsl:value-of select="@id" /></xsl:otherwise>
        </xsl:choose> 
    </xsl:variable>
    <xsl:if test="@virtual = '0'">
    private Dictionary&lt;string,string&gt; Pack<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance)
    {
        var values = GetInitialValues(instance);
        <xsl:apply-templates select="attribute[@type!='M2O']"/>
        return values;
    }
    </xsl:if>

    partial void Insert<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance) 
    {	
    <xsl:choose><xsl:when test="@virtual = '0'">
        Cnn.ExternalTransaction = Transaction;
        var values = Pack<xsl:value-of select="$name" />(instance);
        Cnn.MassUpdate(<xsl:value-of select="$content_id" />, new List&lt;Dictionary&lt;string, string&gt;&gt;() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = <xsl:value-of select="parent::node()/@replaceUrls" />});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;
    </xsl:when><xsl:otherwise>
        throw new InvalidOperationException(@"Virtual Contents cannot be modified");	
    </xsl:otherwise></xsl:choose>
    }

    partial void Update<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance)
    {	
    <xsl:choose><xsl:when test="@virtual = '0'">
        Cnn.ExternalTransaction = Transaction;
        var values = Pack<xsl:value-of select="$name" />(instance);
        Cnn.MassUpdate(<xsl:value-of select="$content_id" />, new List&lt;Dictionary&lt;string, string&gt;&gt;() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = <xsl:value-of select="parent::node()/@replaceUrls" />});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
    </xsl:when><xsl:otherwise>
        throw new InvalidOperationException(@"Virtual Contents cannot be modified");	
    </xsl:otherwise></xsl:choose>		
    }			

    partial void Delete<xsl:value-of select="$name" />(<xsl:value-of select="$name" /> instance)
    {
    <xsl:choose><xsl:when test="@virtual = '0'">
        Cnn.ExternalTransaction = Transaction;			
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));
    </xsl:when><xsl:otherwise>
        throw new InvalidOperationException(@"Virtual Contents cannot be modified");	
    </xsl:otherwise></xsl:choose>		
    }
 </xsl:template>
 
<xsl:template match="attribute">
    <xsl:variable name="content_name" select="parent::node()/@mapped_name" />
    <xsl:variable name="dbIndependent" select="parent::node()/parent::node()/@dbIndependent" />
    <xsl:variable name="name">
        <xsl:choose>
            <xsl:when test="@type = 'O2M'"><xsl:value-of select="concat(@mapped_name,'_ID')" /></xsl:when>
            <xsl:otherwise><xsl:value-of select="@mapped_name" /></xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:variable name="form_name">
        <xsl:choose>
            <xsl:when test="$dbIndependent = 'true'">Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "<xsl:value-of select="$content_name" />", "<xsl:value-of select="@mapped_name" />")).Name</xsl:when>
            <xsl:otherwise>"<xsl:value-of select="@name" />"</xsl:otherwise>
        </xsl:choose> 
    </xsl:variable>
        values[<xsl:value-of select="$form_name" />] = <xsl:choose>
        <xsl:when test="@type='Boolean'"
                  >(instance.<xsl:value-of select="$name" />.HasValue) ? (instance.<xsl:value-of select="$name" />.Value ? "1" : "0") : null;</xsl:when>
        <xsl:when test="@type='O2M' or @type='Numeric' or @type='Date' or @type='Time' or @type='DateTime'" 
                  >(instance.<xsl:value-of select="$name" /> != null) ? instance.<xsl:value-of select="$name" />.ToString() : null</xsl:when>
        <xsl:when test="@type='M2M'">instance.<xsl:value-of select="$name" />String</xsl:when>
        <xsl:otherwise>instance.<xsl:value-of select="$name" /></xsl:otherwise>
        </xsl:choose>;
</xsl:template>
</xsl:stylesheet>
