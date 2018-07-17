using System;
using System.ComponentModel.DataAnnotations;
using BookRecommender.DataManipulation;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookRecommender.Models.Database
{
    public class WikiStorageEntry
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Lang { get; set; }

        [Required]
        public string Text { get; set; }

        public WikiStorageEntry() { }

        public WikiStorageEntry(string Id, string Lang, string Text)
        {
            this.Id = Id;
            this.Lang = Lang;
            this.Text = Text;
        }

        public static void Configure(EntityTypeBuilder<WikiStorageEntry> builder)
        {
            builder.HasKey(ws => new { ws.Id, ws.Lang});

            builder.HasIndex(ws => ws.Id);     
        }
    }
}