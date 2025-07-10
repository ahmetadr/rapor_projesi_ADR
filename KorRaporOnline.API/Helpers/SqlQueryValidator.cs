using System;
using System.Text.RegularExpressions;

namespace KorRaporOnline.API.Helpers
{
    public static class SqlQueryValidator
    {
        private static readonly string[] DangerousKeywords = new[]
        {
            "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "EXEC",
            "sp_", "xp_", "UPDATE", "INSERT", "MERGE"
        };

        public static bool IsValidSelectQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            // Temel SQL injection kontrolü
            query = query.ToUpper();
            
            // Sadece SELECT sorgularına izin ver
            if (!query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                return false;

            // Tehlikeli keywordleri kontrol et
            foreach (var keyword in DangerousKeywords)
            {
                if (query.Contains(keyword))
                    return false;
            }

            // Çoklu statement kontrolü
            if (query.Contains(";"))
                return false;

            return true;
        }

        public static string SanitizeQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return string.Empty;

            // SQL injection karakterlerini temizle
            query = Regex.Replace(query, @"[-;']", "");
            
            return query;
        }
    }
}