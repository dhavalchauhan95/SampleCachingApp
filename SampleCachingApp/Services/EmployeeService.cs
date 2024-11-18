using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SampleCachingApp.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace SampleCachingApp.Services
{
    public class EmployeeService
    {
        private readonly SampleCachingAppContext _sampleCachingAppContext;

        public EmployeeService(SampleCachingAppContext sampleCachingAppContext)
        {
            _sampleCachingAppContext = sampleCachingAppContext;
        }

        public List<Employee> GetEmployees(RequestObject obj)
        {
            IQueryable<Employee> employees = null;

            var employeesQueryable = _sampleCachingAppContext.Employee.AsQueryable();

            try
            {
                if (obj.filterCondition.Count > 0)
                {
                    // Collect all the list of filter to apply later based on few conditions like to avoid filters with empty value and improper filteration keyword. 
                    var filterList = new List<FilterCondition>();

                    foreach (var item in obj.filterCondition)
                    {
                        //Checking filteration keyword
                        bool isValidType = IsValidEnumValue(item.Comparison, typeof(FilterationType));
                        if (item.PropertyName != "" && item.Value != "" && isValidType)
                        {
                            FilterationType type = (FilterationType)Enum.Parse(typeof(FilterationType), item.Comparison, true);
                            object value = null;

                            Type employeeType = typeof(Employee);
                            PropertyInfo propertyInfo = employeeType.GetProperty(item.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            
                            //In operatior is used to filter multiple values with comma seperated value
                            if (type == FilterationType.In)
                            {
                                value = propertyInfo?.PropertyType == typeof(int) ? Array.ConvertAll(item.Value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray(), int.Parse) :
                                    item.Value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                            }
                            else
                            {
                                value = propertyInfo?.PropertyType == typeof(int) ? Convert.ToInt32(item.Value) : item.Value;
                            }


                            bool exists = filterList.Any(val => val.PropertyName == item.PropertyName && val.Comparison == type);
                            //If already a filter is added with same operation, same field and value then this code will avoid duplication 
                            if (!exists)
                            {
                                filterList.Add(new FilterCondition
                                {
                                    PropertyName = item.PropertyName,
                                    Value = value,
                                    Comparison = type
                                });
                            }
                        }
                    }
                    if (filterList.Count > 0)
                    {
                        //Using extension method to get IQueryable expression as a result
                        employees = employeesQueryable.DynamicFilters(filterList);
                    }

                }
            }
            catch
            {
                throw new ArgumentException("Request parameters are incorrect.");
            }
            //Using extension method to get IQueryable expression as a result
            employees = employees.DynamicSorting(obj.sortingParameters).Skip((obj.paging.PageNo - 1) * obj.paging.PageSize).Take(obj.paging.PageSize);

            //Performing whole operation in DB
            return employees.ToList();
        }
        public static bool IsValidEnumValue(string value, Type enumType)
        {
            return Enum.TryParse(enumType, value, true, out _);
        }
    }
}
