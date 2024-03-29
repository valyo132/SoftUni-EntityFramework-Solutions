﻿namespace BookShop
{
    using System.Linq;
    using System.Text;
    using static System.Reflection.Metadata.BlobBuilder;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

    using BookShop.Models;
    using Data;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using BookShop.Initializer;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            // Problem 16
            //var result = RemoveBooks(db);

            // Problem 15
            //IncreasePrices(db);

            // Problem 14
            //var result = GetMostRecentBooks(db);

            // Problem 13
            //var result = GetTotalProfitByCategory(db);

            // Problem 12
            //var result = CountCopiesByAuthor(db);

            // Problem 11
            //int number = int.Parse(Console.ReadLine());
            //var result = CountBooks(db, number);

            // Problem 10
            //string input = Console.ReadLine();
            //var result = GetBooksByAuthor(db, input);

            // Problem 09
            //string input = Console.ReadLine();
            //var result = GetBookTitlesContaining(db, input);

            // Problem 08
            //string input = Console.ReadLine();
            //var result = GetAuthorNamesEndingIn(db, input);

            // Problem 07
            //string date = Console.ReadLine();
            //var result = GetBooksReleasedBefore(db, date);

            // Problem 06
            //string categories = Console.ReadLine();
            //var result = GetBooksByCategory(db, categories);

            // Problem 05
            //int year = int.Parse(Console.ReadLine());
            //var result = GetBooksNotReleasedIn(db, year);

            // Problem 04
            //var result = GetBooksByPrice(db);

            // Problem 03
            //var result = GetGoldenBooks(db);

            // Problem 02
            //string ageRestriction = Console.ReadLine();
            //var result = GetBooksByAgeRestriction(db, ageRestriction);

            //Console.WriteLine(result);
        }

        // Problem 16
        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Copies < 4200)
                .ToList();

            context.Books.RemoveRange(books);

            context.SaveChanges();

            return books.Count();
        }

        // Problem 15
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in books)
            {
                book.Price += 5;

                context.SaveChanges();
            }
        }

        // Problem 14
        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var allCategories = context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new 
                { 
                    Category = c.Name,
                    Books = c.CategoryBooks
                        .Select(c => new 
                        {
                            Title = c.Book.Title,
                            ReleaseDate = c.Book.ReleaseDate,
                        }).OrderByDescending(c => c.ReleaseDate)
                        .Take(3)
                        .ToList()
                })
                .ToList();

            foreach (var category in allCategories)
            {
                sb.AppendLine($"--{category.Category}");

                foreach (var book in category.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.ReleaseDate.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 13
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var totalSumOfAllCategoires = context.Categories
                .AsNoTracking()
                .OrderByDescending(c => c.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price))
                .ThenBy(c => c.Name)
                .Select(c => $"{c.Name} ${c.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price):f2}")
                .ToList();

            return string.Join(Environment.NewLine, totalSumOfAllCategoires);
        }

        // Problem 12
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var result = context.Authors
                .AsNoTracking()
                .OrderByDescending(a => a.Books.Sum(b => b.Copies))
                .Select(a => $"{a.FirstName} {a.LastName} - {a.Books.Sum(b => b.Copies)}")
                .ToList();

            return string.Join(Environment.NewLine, result);
        }

        // Problem 11
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .AsNoTracking()
                .Where(b => b.Title.Length > lengthCheck)
                .ToList();

            return books.Count;
        }

        // Problem 10
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .AsNoTracking()
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(b => $"{b.Title} ({b.Author.FirstName} {b.Author.LastName})")
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        // Problem 09
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .AsNoTracking()
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        // Problem 08
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .AsNoTracking()
                .AsEnumerable()
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => $"{a.FirstName} {a.LastName}")
                .OrderBy(a => a)
                .ToList();

            return string.Join(Environment.NewLine, authors);
        }

        // Problem 07
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var datetime = DateTime.ParseExact(date, "dd-MM-yyyy", null);

            var books = context.Books
                .AsNoTracking()
                .Where(b => b.ReleaseDate < datetime)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => $"{b.Title} - {b.EditionType} - ${b.Price:f2}")
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        // Problem 06
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] categories = input.Split().ToArray();

            var books = new List<string>();

            foreach (string category in categories)
            {
                var booksToAdd = context.Books
                    .AsNoTracking()
                    .Where(b => b.BookCategories.All(bc => bc.Category.Name.ToLower() == category.ToLower()))
                    .Select(b => b.Title)
                    .ToList();
                books.AddRange(booksToAdd);
            }

            return string.Join(Environment.NewLine, books.OrderBy(b => b));
        }

        // Problem 05
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .AsNoTracking()
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        // Problem 04
        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .AsNoTracking()
                .AsEnumerable()
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .Select(b => $"{b.Title} - ${b.Price:f2}")
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        // Problem 03
        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.EditionType == Models.Enums.EditionType.Gold && b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        // Problem 02
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var books = context.Books
                .AsNoTracking()
                .AsEnumerable()
                .Where(b => b.AgeRestriction.ToString().ToLower() == command.ToLower())
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }
    }
}


