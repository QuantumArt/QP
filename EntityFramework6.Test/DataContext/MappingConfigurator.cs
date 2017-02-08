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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<AfiellFieldsItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<Schema>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<Schema>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<StringItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<StringItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<StringItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<StringItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<StringItemForUnsert>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<StringItemForUnsert>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<StringItemForUnsert>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);

            modelBuilder.Entity<StringItemForUnsert>()
                .Property(x => x.StringValue)
                .HasColumnName(GetFieldName("StringItemForUnsert", "StringValue"));
            #endregion

            #region ItemForUpdate mappings
            modelBuilder.Entity<ItemForUpdate>()
                .ToTable(GetTableName("ItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<ItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<ItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<ItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region ItemForInsert mappings
            modelBuilder.Entity<ItemForInsert>()
                .ToTable(GetTableName("ItemForInsert"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<ItemForInsert>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<ItemForInsert>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<ItemForInsert>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region PublishedNotPublishedItem mappings
            modelBuilder.Entity<PublishedNotPublishedItem>()
                .ToTable(GetTableName("PublishedNotPublishedItem"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<PublishedNotPublishedItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<PublishedNotPublishedItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<ReplacingPlaceholdersItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<ReplacingPlaceholdersItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<FileFieldsItem>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<FileFieldsItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<SymmetricRelationArticle>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<SymmetricRelationArticle>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<ToSymmetricRelationAtricle>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<ToSymmetricRelationAtricle>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

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
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("ToSymmetricRelationAtricle", "ToSymmetricRelation"));
                });
            #endregion

            #region MtMItemForUpdate mappings
            modelBuilder.Entity<MtMItemForUpdate>()
                .ToTable(GetTableName("MtMItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<MtMItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<MtMItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<MtMItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);


            modelBuilder.Entity<MtMItemForUpdate>().HasMany<MtMDictionaryForUpdate>(p => p.Reference).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("MtMItemForUpdate", "Reference"));
                });

            modelBuilder.Entity<MtMDictionaryForUpdate>().HasMany<MtMItemForUpdate>(p => p.BackwardForReference).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("MtMItemForUpdate", "Reference"));
                });
            #endregion

            #region MtMDictionaryForUpdate mappings
            modelBuilder.Entity<MtMDictionaryForUpdate>()
                .ToTable(GetTableName("MtMDictionaryForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<MtMDictionaryForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<MtMDictionaryForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<MtMDictionaryForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region OtMItemForUpdate mappings
            modelBuilder.Entity<OtMItemForUpdate>()
                .ToTable(GetTableName("OtMItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<OtMItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<OtMItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<OtMItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);

            modelBuilder.Entity<OtMItemForUpdate>()
                .HasOptional<OtMDictionaryForUpdate>(mp => mp.Reference)
                .WithMany(mp => mp.BackReference)
                .HasForeignKey(fp => fp.Reference_ID);

            modelBuilder.Entity<OtMItemForUpdate>()
                .Property(x => x.Reference_ID)
                .HasColumnName(GetFieldName("OtMItemForUpdate", "Reference"));
            #endregion

            #region OtMDictionaryForUpdate mappings
            modelBuilder.Entity<OtMDictionaryForUpdate>()
                .ToTable(GetTableName("OtMDictionaryForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<OtMDictionaryForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<OtMDictionaryForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<OtMDictionaryForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region DateItemForUpdate mappings
            modelBuilder.Entity<DateItemForUpdate>()
                .ToTable(GetTableName("DateItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<DateItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<DateItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<DateItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region TimeItemForUpdate mappings
            modelBuilder.Entity<TimeItemForUpdate>()
                .ToTable(GetTableName("TimeItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<TimeItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<TimeItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<TimeItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region DateTimeItemForUpdate mappings
            modelBuilder.Entity<DateTimeItemForUpdate>()
                .ToTable(GetTableName("DateTimeItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<DateTimeItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<DateTimeItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<DateTimeItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);
            #endregion

            #region FileItemForUpdate mappings
            modelBuilder.Entity<FileItemForUpdate>()
                .ToTable(GetTableName("FileItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<FileItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<FileItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<FileItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);

            modelBuilder.Entity<FileItemForUpdate>().Ignore(p => p.FileValueFieldUrl);
            modelBuilder.Entity<FileItemForUpdate>().Ignore(p => p.FileValueFieldUploadPath);
            #endregion

            #region ImageItemForUpdate mappings
            modelBuilder.Entity<ImageItemForUpdate>()
                .ToTable(GetTableName("ImageItemForUpdate"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName(FieldName.ContentItemId);

		    modelBuilder.Entity<ImageItemForUpdate>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName(FieldName.LastModifiedBy);

            modelBuilder.Entity<ImageItemForUpdate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName(FieldName.StatusTypeId);

			modelBuilder.Entity<ImageItemForUpdate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId);

            modelBuilder.Entity<ImageItemForUpdate>().Ignore(p => p.ImageValueFieldUrl);
            modelBuilder.Entity<ImageItemForUpdate>().Ignore(p => p.ImageValueFieldUploadPath);
            #endregion
        }
    }
}
