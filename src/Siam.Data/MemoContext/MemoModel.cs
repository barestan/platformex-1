using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Siam.Application;

namespace Siam.Data.MemoContext
{
    public class Memo
    {
        [Key]
        public Guid Id { get; set; }

        public MemoModel Model { get; set; }

    }

    public class MemoModelConfiguration : IEntityTypeConfiguration<Memo>
    {
        public void Configure(EntityTypeBuilder<Memo> builder)
        {
            builder.Property(e => e.Model)
                .HasMaxLength(int.MaxValue)
                .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<MemoModel>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }

}
