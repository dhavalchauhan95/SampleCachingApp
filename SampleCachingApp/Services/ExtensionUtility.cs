using SampleCachingApp.Model;
using System.Linq.Expressions;
using System.Reflection;

namespace SampleCachingApp.Services
{
    public static class ExtensionUtility
    {
        public static IQueryable<T> DynamicFilters<T>(this IQueryable<T> query, List<FilterCondition> filters)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            Expression combinedExpression = null;
            try
            {
                //Code to add each filter with certain data type validation based on the requirement.
                foreach (var filter in filters)
                {
                    var property = Expression.Property(parameter, filter.PropertyName);
                    var constant = Expression.Constant(filter.Value);

                    Expression condition;
                    switch (filter.Comparison)
                    {
                        case FilterationType.Equals:
                            condition = Expression.Equal(property, constant);
                            break;

                        case FilterationType.Contains:
                            if (filter.Value is string)
                            {
                                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                var propertyToLower = Expression.Call(property, toLowerMethod);
                                var constantToLower = Expression.Call(constant, toLowerMethod);
                                condition = Expression.Call(propertyToLower, containsMethod, constantToLower);
                            }
                            else
                            {
                                throw new ArgumentException("Contains comparison can only be used with string properties.");
                            }
                            break;

                        case FilterationType.StartsWith:
                            if (filter.Value is string)
                            {
                                var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                                condition = Expression.Call(property, startsWithMethod, constant);
                            }
                            else
                            {
                                throw new ArgumentException("StartsWith comparison can only be used with string properties.");
                            }
                            break;

                        case FilterationType.EndsWith:
                            if (filter.Value is string)
                            {
                                var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                                condition = Expression.Call(property, endsWithMethod, constant);
                            }
                            else
                            {
                                throw new ArgumentException("EndsWith comparison can only be used with string properties.");
                            }
                            break;

                        case FilterationType.In:
                            if (filter.Value is IEnumerable<string> stringList)
                            {
                                var containsMethodForList = typeof(Enumerable).GetMethods()
                                    .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(typeof(string));



                                var stringListExpression = Expression.Constant(stringList.Select(s => s.ToLower()).ToList());
                                var lowerProperty = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

                                condition = Expression.Call(containsMethodForList, stringListExpression, lowerProperty);
                            }
                            else if (filter.Value is IEnumerable<int> intList)
                            {
                                var containsMethodForList = typeof(Enumerable).GetMethods()
                                    .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(typeof(int));
                                var intListExpression = Expression.Constant(intList);
                                condition = Expression.Call(containsMethodForList, intListExpression, property);
                            }
                            else
                            {
                                throw new ArgumentException("In comparison must be used with a list of strings.");
                            }
                            break;

                        case FilterationType.LessThan:
                            if (property.Type == typeof(int) || property.Type == typeof(double) || property.Type == typeof(decimal))
                            {
                                condition = Expression.LessThan(property, constant);
                            }
                            else
                            {
                                throw new ArgumentException("LessThan comparison can only be used with numeric properties.");
                            }
                            break;

                        case FilterationType.GreaterThan:
                            if (property.Type == typeof(int) || property.Type == typeof(double) || property.Type == typeof(decimal))
                            {
                                condition = Expression.GreaterThan(property, constant);
                            }
                            else
                            {
                                throw new ArgumentException("GreaterThan comparison can only be used with numeric properties.");
                            }
                            break;

                        default:
                            throw new NotSupportedException($"Comparison type {filter.Comparison} is not supported.");
                    }

                    combinedExpression = combinedExpression == null
                        ? condition
                        : Expression.AndAlso(combinedExpression, condition);
                }
            }
            catch
            {
                throw new ArgumentException("Request parameters are incorrect.");
            }
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> DynamicSorting<T>(this IQueryable<T> query, SortingParameters sortingParameters)
        {
            if (string.IsNullOrEmpty(sortingParameters?.sortProperty))
            {
                return query; // Return the original query if no sorting is specified
            }

            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyInfo = typeof(T).GetProperty(sortingParameters.sortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{sortingParameters.sortProperty}' not found on type '{typeof(T).Name}'. Ensure the name matches a valid property.");
            }

            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);

            string methodName = sortingParameters.ascendingSort ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyInfo.PropertyType);

            return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda });
        }

    }

}
