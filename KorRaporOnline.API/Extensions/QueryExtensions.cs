using System;
using System.Linq;
using System.Linq.Expressions;
using KorRaporOnline.API.Models.DTOs;

namespace KorRaporOnline.API.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(
            this IQueryable<T> query,
            int page,
            int pageSize)
        {
            pageSize = pageSize > ValidationConstants.MAX_PAGE_SIZE
                ? ValidationConstants.MAX_PAGE_SIZE
                : pageSize;

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> query,
            string searchTerm,
            params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || propertyNames == null || !propertyNames.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression combinedExpression = null;

            foreach (var propertyName in propertyNames)
            {
                try
                {
                    // Property expression: x.PropertyName
                    var property = Expression.Property(parameter, propertyName);

                    // Convert to string: x.PropertyName.ToString()
                    var toStringCall = Expression.Call(property, "ToString", Type.EmptyTypes);

                    // ToLower: x.PropertyName.ToString().ToLower()
                    var toLowerCall = Expression.Call(toStringCall, "ToLower", Type.EmptyTypes);

                    // Contains: x.PropertyName.ToString().ToLower().Contains(searchTerm.ToLower())
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var searchTermExpression = Expression.Constant(searchTerm.ToLower());
                    var containsCall = Expression.Call(toLowerCall, containsMethod, searchTermExpression);

                    // Combine with OR
                    combinedExpression = combinedExpression == null
                        ? containsCall
                        : Expression.OrElse(combinedExpression, containsCall);
                }
                catch (ArgumentException)
                {
                    // Property bulunamazsa bu property'yi atla
                    continue;
                }
            }

            if (combinedExpression == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }
    }
}