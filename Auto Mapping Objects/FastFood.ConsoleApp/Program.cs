using FastFood.Data;

namespace FastFood.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FastFoodContext context = new FastFoodContext();

            var employees = context.Employees
                .Select(e => e.Name)
                .ToArray();

            Console.WriteLine(string.Join(Environment.NewLine, employees));
        }
    }
}