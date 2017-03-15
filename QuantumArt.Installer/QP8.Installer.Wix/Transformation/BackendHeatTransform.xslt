<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>

  <!--Identity Transform-->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!--Set up keys of component Ids from Snapshot.xml-->
  <!--NB: Reinstate the line below when everyone has upgraded to Wix 3.6-->
  <!--<xsl:key name="snapshot-search" match="wix:Component[@Id = document('Snapshot.xml')//wix:Component/@Id]" use="@Id"/>-->

  <!-- Set up keys for ignoring various file types -->
  <!--<xsl:key name="config-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.config')) + 1) = '.config']" use="@Id"/>-->
  <xsl:key name="cs-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.cs')) + 1) = '.cs']" use="@Id"/>
  <xsl:key name="csproj-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.csproj')) + 1) = '.csproj']" use="@Id"/>
  <xsl:key name="proj-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.proj')) + 1) = '.proj']" use="@Id"/>
  <xsl:key name="tt-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.tt')) + 1) = '.tt']" use="@Id"/>
  <xsl:key name="t4-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.t4')) + 1) = '.t4']" use="@Id"/>
  <xsl:key name="user-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.csproj.user')) + 1) = '.csproj.user']" use="@Id"/>
  <xsl:key name="vspscc-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.vspscc')) + 1) = '.vspscc']" use="@Id"/>
  <xsl:key name="t4mvc0-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('T4MVC.tt.settings.xml')) + 1) = 'T4MVC.tt.settings.xml']" use="@Id"/>
  <xsl:key name="obj-search" match="wix:Component[contains(wix:File/@Source, '\obj\Debug\')]" use="@Id"/>
  <xsl:key name="bin-xml-search" match="wix:Component[contains(wix:File/@Source, '\bin\') and substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.xml')) + 1) = '.xml']" use="@Id"/>
  <xsl:key name="bin-xaml-search" match="wix:Component[contains(wix:File/@Source, '\bin\') and substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('.xaml')) + 1) = '.xaml']" use="@Id"/>
  <xsl:key name="prop-search" match="wix:Component[contains(wix:File/@Source, '\Properties\')]" use="@Id"/>
  <xsl:key name="conf-storage-search" match="wix:Component[contains(wix:File/@Source, '\ConfigurationStorage.')]" use="@Id"/>
  <xsl:key name="conf-packages-config-search" match="wix:Component[contains(wix:File/@Source, 'packages.config')]" use="@Id"/>

  <xsl:key name="config-debug-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('Debug.config')) + 1) = 'Debug.config']" use="@Id"/>
  <xsl:key name="config-release-search" match="wix:Component[substring(wix:File/@Source, (string-length(wix:File/@Source) - string-length('Release.config')) + 1) = 'Release.config']" use="@Id"/>
  <!-- Match and ignore .config files -->
  <!--<xsl:template match="wix:Component[key('config-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('config-search', @Id)]"/>-->

  <!-- Match and ignore .cs files -->

  <xsl:template match="wix:Component[key('cs-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('cs-search', @Id)]"/>

  <xsl:template match="wix:Component[key('config-debug-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('config-debug-search', @Id)]"/>
  
  <xsl:template match="wix:Component[key('config-release-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('config-release-search', @Id)]"/>

  <xsl:template match="wix:Component[key('csproj-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('csproj-search', @Id)]"/>
  
  <xsl:template match="wix:Component[key('proj-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('proj-search', @Id)]"/>

  <xsl:template match="wix:Component[key('tt-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('tt-search', @Id)]"/>

  <xsl:template match="wix:Component[key('t4-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('t4-search', @Id)]"/>

  <xsl:template match="wix:Component[key('user-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('user-search', @Id)]"/>

  <xsl:template match="wix:Component[key('vspscc-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('vspscc-search', @Id)]"/>

  <xsl:template match="wix:Component[key('t4mvc0-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('t4mvc0-search', @Id)]"/>

  <xsl:template match="wix:Component[key('obj-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('obj-search', @Id)]"/>

  <xsl:template match="wix:Component[key('bin-xml-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('bin-xml-search', @Id)]"/>

  <xsl:template match="wix:Component[key('bin-xaml-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('bin-xaml-search', @Id)]"/>

  <xsl:template match="wix:Component[key('prop-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('prop-search', @Id)]"/>
  
  <xsl:template match="wix:Component[key('conf-storage-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('conf-storage-search', @Id)]"/>
  
  <xsl:template match="wix:Component[key('conf-packages-config-search', @Id)]"/>
  <xsl:template match="wix:ComponentRef[key('conf-packages-config-search', @Id)]"/>
  
  <!--Match Components that also exist in Snapshot.xml, and use the snapshot version-->
  <!--NB: Reinstate the 4 lines below when everyone has upgraded to Wix 3.6-->
  <!--<xsl:template match="wix:Component[key('snapshot-search', @Id)]">
  <xsl:variable name="component" select="."/>
  <xsl:copy-of select="document(‘Snapshot.xml’)//wix:Component[@Id = $component/@Id]"/>
</xsl:template>-->

</xsl:stylesheet>