using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using Quantumart.QP8.CodeGeneration.Services;


namespace EntityFramework6.Test.DataContext
{
    public class MappingConfigurator : MappingConfiguratorBase
    {
        public MappingConfigurator(ContentAccess contentAccess, ISchemaProvider schemaProvider)
            : base(contentAccess, schemaProvider)
        {
		}
       
        public override void OnModelCreating(DbModelBuilder modelBuilder)
        {
			base.OnModelCreating(modelBuilder);

            #region AfiellFieldsItem mappings
            modelBuilder.Entity<AfiellFieldsItem>()
                .ToTable(GetTableName("AfiellFieldsItem"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<AfiellFieldsItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.String)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "String"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Integer)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Integer"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Decimal)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Decimal"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Boolean)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Boolean"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Date)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Date"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Time)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Time"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.DateTime)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "DateTime"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.File)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "File"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Image)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Image"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.TextBox)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "TextBox"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.VisualEdit)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "VisualEdit"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.DynamicImage)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "DynamicImage"));
            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.Enum)
                .HasColumnName(GetFieldName("AfiellFieldsItem", "Enum"));
            modelBuilder.Entity<AfiellFieldsItem>().Ignore(p => p.FileUrl);
            modelBuilder.Entity<AfiellFieldsItem>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<AfiellFieldsItem>().Ignore(p => p.DynamicImageUrl);
            modelBuilder.Entity<AfiellFieldsItem>().Ignore(p => p.FileUploadPath);
            modelBuilder.Entity<AfiellFieldsItem>().Ignore(p => p.ImageUploadPath);
 
            #endregion

            #region Schema mappings
            modelBuilder.Entity<Schema>()
                .ToTable(GetTableName("Schema"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Schema>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Schema>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Schema>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

 
            #endregion

            #region StringItem mappings
            modelBuilder.Entity<StringItem>()
                .ToTable(GetTableName("StringItem"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<StringItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<StringItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<StringItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<StringItem>()
                .Property(x => x.StringValue)
                .HasColumnName(GetFieldName("StringItem", "StringValue"));
 
            #endregion

            #region StringItemForUpdate mappings
            modelBuilder.Entity<StringItemForUpdate>()
                .ToTable(GetTableName("StringItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<StringItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<StringItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<StringItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<StringItemForUpdate>()
                .Property(x => x.StringValue)
                .HasColumnName(GetFieldName("StringItemForUpdate", "StringValue"));
 
            #endregion

            #region StringItemForUnsert mappings
            modelBuilder.Entity<StringItemForUnsert>()
                .ToTable(GetTableName("StringItemForUnsert"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<StringItemForUnsert>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<StringItemForUnsert>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<StringItemForUnsert>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<StringItemForUnsert>()
                .Property(x => x.StringValue)
                .HasColumnName(GetFieldName("StringItemForUnsert", "StringValue"));
 
            #endregion

            #region PublishedNotPublishedItem mappings
            modelBuilder.Entity<PublishedNotPublishedItem>()
                .ToTable(GetTableName("PublishedNotPublishedItem"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PublishedNotPublishedItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PublishedNotPublishedItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PublishedNotPublishedItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

 
            #endregion

            #region ReplacingPlaceholdersItem mappings
            modelBuilder.Entity<ReplacingPlaceholdersItem>()
                .ToTable(GetTableName("ReplacingPlaceholdersItem"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ReplacingPlaceholdersItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ReplacingPlaceholdersItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ReplacingPlaceholdersItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

 
            #endregion

            #region FileFieldsItem mappings
            modelBuilder.Entity<FileFieldsItem>()
                .ToTable(GetTableName("FileFieldsItem"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FileFieldsItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FileFieldsItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FileFieldsItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<FileFieldsItem>().Ignore(p => p.FileItemUrl);
            modelBuilder.Entity<FileFieldsItem>().Ignore(p => p.FileItemUploadPath);
 
            #endregion

            #region SymmetricRelationArticle mappings
            modelBuilder.Entity<SymmetricRelationArticle>()
                .ToTable(GetTableName("SymmetricRelationArticle"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SymmetricRelationArticle>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SymmetricRelationArticle>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SymmetricRelationArticle>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<SymmetricRelationArticle>().HasMany<ToSymmetricRelationAtricle>(p => p.SymmetricRelation).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("SymmetricRelationArticle", "SymmetricRelation"));
                });

            modelBuilder.Entity<ToSymmetricRelationAtricle>().HasMany<SymmetricRelationArticle>(p => p.BackwardForSymmetricRelation).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("SymmetricRelationArticle", "SymmetricRelation"));
                });

 
            #endregion

            #region ToSymmetricRelationAtricle mappings
            modelBuilder.Entity<ToSymmetricRelationAtricle>()
                .ToTable(GetTableName("ToSymmetricRelationAtricle"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ToSymmetricRelationAtricle>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ToSymmetricRelationAtricle>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ToSymmetricRelationAtricle>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<ToSymmetricRelationAtricle>().HasMany<SymmetricRelationArticle>(p => p.ToSymmetricRelation).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("ToSymmetricRelationAtricle", "ToSymmetricRelation"));
                });

            modelBuilder.Entity<SymmetricRelationArticle>().HasMany<ToSymmetricRelationAtricle>(p => p.BackwardForToSymmetricRelation).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("ToSymmetricRelationAtricle", "ToSymmetricRelation"));
                });

 
            #endregion
        }
    }
}
