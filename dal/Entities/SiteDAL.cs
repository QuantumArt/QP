using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{
    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class SiteDAL : IQpEntityObject
    {
        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Dns { get; set; }
        public string LiveDirectory { get; set; }
        public string LiveVirtualRoot { get; set; }
        public string StageDirectory { get; set; }
        public string StageVirtualRoot { get; set; }
        public string IsLive { get; set; }
        public string Description { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public string UploadDir { get; set; }
        public string UploadUrl { get; set; }
        public decimal AllowUserSessions { get; set; }
        public decimal UseAbsoluteUploadUrl { get; set; }
        public string UploadUrlPrefix { get; set; }
        public string ScriptLanguage { get; set; }
        public decimal OnScreenFieldBorder { get; set; }
        public decimal OnScreenObjectBorder { get; set; }
        public decimal OnScreenObjectTypeMask { get; set; }
        public Nullable<System.DateTime> Locked { get; set; }
        public Nullable<decimal> LockedBy { get; set; }
        public decimal ForceAssemble { get; set; }
        public string AssemblyPath { get; set; }
        public string StageDns { get; set; }
        public string StageAssemblyPath { get; set; }
        public bool AssembleFormatsInLive { get; set; }
        public string TestDirectory { get; set; }
        public bool ForceTestDirectory { get; set; }
        public bool ImportMappingToDb { get; set; }
        public bool ProceedMappingWithDb { get; set; }
        public bool ReplaceUrls { get; set; }
        public bool UseLongUrls { get; set; }
        public string Namespace { get; set; }
        public string ConnectionStringName { get; set; }
        public string ContextClassName { get; set; }
        public bool PEnterMode { get; set; }
        public bool ProceedDbIndependentGeneration { get; set; }
        public bool GenerateMapFileOnly { get; set; }
        public bool EnableOnScreen { get; set; }
        public bool PermanentLock { get; set; }
        public bool UseEnglishQuotes { get; set; }
        public bool ReplaceUrlsInDB { get; set; }
        public string ExternalUrl { get; set; }
        public bool SendNotifications { get; set; }
        public string ExternalCss { get; set; }
        public string RootElementClass { get; set; }
        public string XamlDictionaries { get; set; }
        public string ContentFormScript { get; set; }
        public bool ExternalDevelopment { get; set; }
        public bool DownloadEfSource { get; set; }
        public bool DisableListAutoWrap { get; set; }

        public ICollection<CodeSnippetDAL> CodeSnippets { get; set; }
        public ICollection<ContentDAL> Contents { get; set; }
        public ICollection<ContentGroupDAL> ContentGroups { get; set; }
        public ICollection<SiteFolderDAL> Folders { get; set; }
        public ICollection<PageTemplateDAL> PageTemplates { get; set; }
        public ICollection<SitePermissionDAL> AccessRules { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
        public UserDAL LockedByUser { get; set; }
        public ICollection<StatusTypeDAL> Statuses { get; set; }
        public ICollection<StyleDAL> Styles { get; set; }
        public ICollection<WorkflowDAL> Workflows { get; set; }

        public ICollection<SiteCustomActionBindDAL> SiteCustomActionBinds { get; set; }

        public ICollection<PluginFieldValueDAL> PluginFieldValues { get; set; }

        [NotMapped]
        public IEnumerable<CustomActionDAL> CustomActions => SiteCustomActionBinds?.Select(x => x.CustomAction);
    }

    public class SiteDALConfiguration : IEntityTypeConfiguration<SiteDAL>
    {
        public void Configure(EntityTypeBuilder<SiteDAL> builder)
        {
            builder.ToTable("SITE");

            builder.Property(x => x.DisableListAutoWrap).HasColumnName("DISABLE_LIST_AUTO_WRAP");
            builder.Property(x => x.DownloadEfSource).HasColumnName("DOWNLOAD_EF_SOURCE");
            builder.Property(x => x.ExternalDevelopment).HasColumnName("EXTERNAL_DEVELOPMENT");
            builder.Property(x => x.ContentFormScript).HasColumnName("CONTENT_FORM_SCRIPT");
            builder.Property(x => x.XamlDictionaries).HasColumnName("XAML_DICTIONARIES");
            builder.Property(x => x.RootElementClass).HasColumnName("ROOT_ELEMENT_CLASS");
            builder.Property(x => x.ExternalCss).HasColumnName("EXTERNAL_CSS");
            builder.Property(x => x.SendNotifications).HasColumnName("SEND_NOTIFICATIONS");
            builder.Property(x => x.ExternalUrl).HasColumnName("EXTERNAL_URL");
            builder.Property(x => x.Id).HasColumnName("SITE_ID").ValueGeneratedOnAdd();
            builder.Property(x => x.Name).HasColumnName("SITE_NAME");
            builder.Property(x => x.Dns).HasColumnName("DNS");
            builder.Property(x => x.LiveDirectory).HasColumnName("LIVE_DIRECTORY");
            builder.Property(x => x.LiveVirtualRoot).HasColumnName("LIVE_VIRTUAL_ROOT");
            builder.Property(x => x.StageDirectory).HasColumnName("STAGE_DIRECTORY");
            builder.Property(x => x.StageVirtualRoot).HasColumnName("STAGE_VIRTUAL_ROOT");
            builder.Property(x => x.IsLive).HasColumnName("IS_LIVE");
            builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
            builder.Property(x => x.UploadDir).HasColumnName("UPLOAD_DIR");
            builder.Property(x => x.UploadUrl).HasColumnName("UPLOAD_URL");
            builder.Property(x => x.AllowUserSessions).HasColumnName("ALLOW_USER_SESSIONS");
            builder.Property(x => x.UseAbsoluteUploadUrl).HasColumnName("use_absolute_upload_url");
            builder.Property(x => x.UploadUrlPrefix).HasColumnName("upload_url_prefix");
            builder.Property(x => x.ScriptLanguage).HasColumnName("script_language");
            builder.Property(x => x.OnScreenFieldBorder).HasColumnName("stage_edit_field_border");
            builder.Property(x => x.OnScreenObjectBorder).HasColumnName("stage_edit_object_border");
            builder.Property(x => x.OnScreenObjectTypeMask).HasColumnName("stage_edit_object_type_mask");
            builder.Property(x => x.Locked).HasColumnName("LOCKED");
            builder.Property(x => x.LockedBy).HasColumnName("LOCKED_BY");
            builder.Property(x => x.ForceAssemble).HasColumnName("FORCE_ASSEMBLE");
            builder.Property(x => x.AssemblyPath).HasColumnName("ASSEMBLY_PATH");
            builder.Property(x => x.StageDns).HasColumnName("STAGE_DNS");
            builder.Property(x => x.StageAssemblyPath).HasColumnName("STAGE_ASSEMBLY_PATH");
            builder.Property(x => x.AssembleFormatsInLive).HasColumnName("ASSEMBLE_FORMATS_IN_LIVE");
            builder.Property(x => x.TestDirectory).HasColumnName("TEST_DIRECTORY");
            builder.Property(x => x.ForceTestDirectory).HasColumnName("FORCE_TEST_DIRECTORY");
            builder.Property(x => x.ImportMappingToDb).HasColumnName("IMPORT_MAPPING_TO_DB");
            builder.Property(x => x.ProceedMappingWithDb).HasColumnName("PROCEED_MAPPING_WITH_DB");
            builder.Property(x => x.ReplaceUrls).HasColumnName("REPLACE_URLS");
            builder.Property(x => x.UseLongUrls).HasColumnName("USE_LONG_URLS");
            builder.Property(x => x.Namespace).HasColumnName("NAMESPACE");
            builder.Property(x => x.ConnectionStringName).HasColumnName("CONNECTION_STRING_NAME");
            builder.Property(x => x.ContextClassName).HasColumnName("CONTEXT_CLASS_NAME");
            builder.Property(x => x.PermanentLock).HasColumnName("PERMANENT_LOCK");
            builder.Property(x => x.PEnterMode).HasColumnName("P_ENTER_MODE");
            builder.Property(x => x.ProceedDbIndependentGeneration).HasColumnName("PROCEED_DB_INDEPENDENT_GENERATION");
            builder.Property(x => x.GenerateMapFileOnly).HasColumnName("GENERATE_MAP_FILE_ONLY");
            builder.Property(x => x.EnableOnScreen).HasColumnName("ENABLE_ONSCREEN");
            builder.Property(x => x.UseEnglishQuotes).HasColumnName("USE_ENGLISH_QUOTES");
            builder.Property(x => x.ReplaceUrlsInDB).HasColumnName("REPLACE_URLS_IN_DB");

            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.CodeSnippets).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.Contents).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.ContentGroups).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.Folders).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.PageTemplates).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.AccessRules).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.LastModifiedSites).HasForeignKey(x => x.LastModifiedBy);
            builder.HasOne(x => x.LockedByUser).WithMany(y => y.LockedSites).HasForeignKey(x => x.LockedBy);
            builder.HasMany(x => x.Statuses).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.Styles).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);
            builder.HasMany(x => x.Workflows).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);

            builder.HasMany(x => x.SiteCustomActionBinds).WithOne(y => y.Site).HasForeignKey(y => y.SiteId);

            // builder.HasMany(x => x.CustomActions).WithMany(y => y.Sites).HasForeignKey(y => y.);
        }
    }
}
