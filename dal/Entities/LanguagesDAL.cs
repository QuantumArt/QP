//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class LanguagesDAL
    {

        public decimal LanguageId { get; set; }
        public string LanguageName { get; set; }
        public decimal LanguagePt { get; set; }
        public decimal Locale { get; set; }
        public decimal Codepage { get; set; }
        public string Charset { get; set; }
        public string GeneralDateFormat { get; set; }
        public string LongDateFormat { get; set; }
        public string ShortDateFormat { get; set; }
        public string LongTimeFormat { get; set; }
        public string ShortTimeFormat { get; set; }
        public bool Direction { get; set; }
        public string IsoCode { get; set; }

        public ICollection<TranslationsDAL> Translations { get; set; }
        public ICollection<UserDAL> Users { get; set; }
    }
        public class LanguagesDALConfiguration : IEntityTypeConfiguration<LanguagesDAL>
        {
            public void Configure(EntityTypeBuilder<LanguagesDAL> builder)
            {
                builder.ToTable("LANGUAGES");

                builder.Property(x => x.LanguageId).HasColumnName("LANGUAGE_ID");
				builder.Property(x => x.LanguageName).HasColumnName("LANGUAGE_NAME");
				builder.Property(x => x.LanguagePt).HasColumnName("LANGUAGE_PT");
				builder.Property(x => x.Locale).HasColumnName("LOCALE");
				builder.Property(x => x.Codepage).HasColumnName("CODEPAGE");
				builder.Property(x => x.Charset).HasColumnName("CHARSET");
				builder.Property(x => x.GeneralDateFormat).HasColumnName("general_date_format");
				builder.Property(x => x.LongDateFormat).HasColumnName("long_date_format");
				builder.Property(x => x.ShortDateFormat).HasColumnName("short_date_format");
				builder.Property(x => x.LongTimeFormat).HasColumnName("long_time_format");
				builder.Property(x => x.ShortTimeFormat).HasColumnName("short_time_format");
				builder.Property(x => x.Direction).HasColumnName("direction");
				builder.Property(x => x.IsoCode).HasColumnName("iso_code");


                builder.HasKey(x => x.LanguageId);

                builder.HasMany(x => x.Translations).WithOne(y => y.Languages).HasForeignKey(y => y.LanguageId);
    			builder.HasMany(x => x.Users).WithOne(y => y.Languages).HasForeignKey(y => y.LanguageId);

            }
        }
}
