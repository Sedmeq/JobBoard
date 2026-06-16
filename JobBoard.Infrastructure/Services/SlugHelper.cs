using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Services
{

    public static class SlugHelper
    {
        public static string Generate(string title)
        {
            var slug = title.ToLowerInvariant().Trim();

            // Azərbaycan/türk xüsusi simvollarını dəyiş
            slug = slug
                .Replace("ə", "e").Replace("ö", "o").Replace("ü", "u")
                .Replace("ğ", "g").Replace("ş", "s").Replace("ç", "c")
                .Replace("ı", "i").Replace("İ", "i");

            // Boşluqları tire ilə əvəz et
            slug = Regex.Replace(slug, @"\s+", "-");

            // Yalnız hərf, rəqəm və tire saxla
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Ardıcıl tireləri bir tirəyə endir
            slug = Regex.Replace(slug, @"\-+", "-");

            return slug.Trim('-');
        }

        public static async Task<string> GenerateUniqueJobSlugAsync(string title, AppDbContext db, int? excludeId = null)
        {
            var baseSlug = Generate(title);
            var slug = baseSlug;
            var counter = 2;

            while (await db.Jobs
                .IgnoreQueryFilters()
                .AnyAsync(j => j.Slug == slug && j.Id != excludeId))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            return slug;
        }

        public static async Task<string> GenerateUniqueBlogSlugAsync(string title, AppDbContext db, int? excludeId = null)
        {
            var baseSlug = Generate(title);
            var slug = baseSlug;
            var counter = 2;

            while (await db.BlogPosts
                .IgnoreQueryFilters()
                .AnyAsync(b => b.Slug == slug && b.Id != excludeId))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            return slug;
        }
    }
}
