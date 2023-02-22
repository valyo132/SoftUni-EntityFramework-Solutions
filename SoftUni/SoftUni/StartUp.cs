using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext context = new SoftUniContext();

            string result = string.Empty;

            // Problem 10
            result = GetDepartmentsWithMoreThan5Employees(context);

            // Problem 09
            // Compile Time Error
            //result = GetEmployee147(context);

            // Problem 08
            //result = GetAddressesByTown(context);

            // Problem 07
            // Compile Time Error
            //result = GetEmployeesInPeriod(context);

            // Problem 06
            //result = AddNewAddressToEmployee(context);

            // Problem 05
            //result = GetEmployeesFromResearchAndDevelopment(context);

            // Problem 04
            //result = GetEmployeesWithSalaryOver50000(context);

            // Problem 03
            //result = GetEmployeesFullInformation(context);

            Console.WriteLine(result);
        }

        // Problem 10
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    DepartmenName = d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
                    Employees = d.Employees
                        .Select(e => new { e.FirstName, e.LastName, e.JobTitle })
                        .OrderBy(e => e.FirstName)
                        .ThenBy(e => e.LastName)
                        .ToList()
                }).ToList();

            foreach (var depart in departments)
            {
                sb.AppendLine($"{depart.DepartmenName} - {depart.ManagerFirstName} {depart.ManagerLastName}");
                sb.Append(string.Join(Environment.NewLine, depart.Employees.Select(e => $"{e.FirstName} {e.LastName} - {e.JobTitle}")));
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 09
        public static string GetEmployee147(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employee = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(a => new
                {
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    JobTitle = a.JobTitle,
                    Projects = a.Projects
                })
                .FirstOrDefault();

            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            foreach (var project in employee.Projects.OrderBy(a => a.Name))
            {
                sb.AppendLine($"{project.Name}");
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 08
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .OrderByDescending(a => a.Employees.Count)
                .ThenBy(a => a.Town.Name)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .Select(a => $"{a.AddressText}, {a.Town.Name} - {a.Employees.Count} employees")
                .ToList();

            return string.Join(Environment.NewLine, addresses);
        }

        // Problem 07
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();
            var employees = context.Employees
                .Where(e => e.Projects.Any(p => p.StartDate.Year >= 2001 && p.StartDate.Year <= 2003))
                .Select(e => new { e.FirstName, e.LastName, e.Projects, e.ManagerId })
                .Take(10)
                .ToList();

            foreach (var employee in employees)
            {
                var manager = context.Employees.Where(e => e.EmployeeId == employee.ManagerId).First();

                sb.AppendLine($"{employee.FirstName} {employee.LastName} - Manager: {manager.FirstName} {manager.LastName}");

                foreach (var project in employee.Projects)
                {
                    var endDateProject = project.EndDate != null
                            ? project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                            : "not finished";

                    var startDate = project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                    sb.AppendLine($"--{project.Name} - {startDate} - {endDateProject}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 06
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var address = new Address() { AddressText = "Vitoshka 15", TownId = 4 };

            context.Addresses.Add(address);

            var employee = context.Employees
                .Where(e => e.LastName == "Nakov")
                .FirstOrDefault();

            employee.Address = address;

            context.SaveChanges();

            var addresses = context.Employees
                .AsNoTracking()
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText)
                .ToList();

            foreach (var item in addresses)
            {
                sb.AppendLine(item);
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 05
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Where(e => e.Department.Name == "Research and Development")
                .OrderBy(e => e.Salary).ThenByDescending(e => e.FirstName)
                .Select(e => new { e.FirstName, e.LastName, e.Department.Name, e.Salary })
                .ToList();


            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.Name} - ${employee.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 04
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Where(e => e.Salary > 50_000)
                .OrderBy(e => e.FirstName)
                .ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 03
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .OrderBy(x => x.EmployeeId)
                .ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}