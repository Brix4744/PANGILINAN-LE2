using BlogDataLibrary.Data;
using BlogDataLibrary.Database;
using BlogDataLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogTestUI
{
    class Program
    {
        static void Main(string[] args)
        {
            // ── Dependency Injection Setup ─────────────────────────────
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddTransient<ISqlDataAccess, SqlDataAccess>();
            services.AddTransient<SqlData>();

            var serviceProvider = services.BuildServiceProvider();
            var db = serviceProvider.GetRequiredService<SqlData>();

            // ── Console Menu ───────────────────────────────────────────
            bool running = true;
            UserModel? loggedInUser = null;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("          BLOG SITE - CONSOLE UI        ");
                Console.WriteLine("========================================");

                if (loggedInUser != null)
                    Console.WriteLine($"  Logged in as: {loggedInUser.FirstName} {loggedInUser.LastName}\n");

                Console.WriteLine("  [1] Register");
                Console.WriteLine("  [2] Log In");
                Console.WriteLine("  [3] List All Posts");
                Console.WriteLine("  [4] View Post Details");

                if (loggedInUser != null)
                    Console.WriteLine("  [5] Create Post");

                Console.WriteLine("  [0] Exit");
                Console.Write("\nChoice: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        RegisterUser(db);
                        break;
                    case "2":
                        loggedInUser = LoginUser(db);
                        break;
                    case "3":
                        ListAllPosts(db);
                        break;
                    case "4":
                        ViewPostDetails(db);
                        break;
                    case "5":
                        if (loggedInUser != null)
                            CreatePost(db, loggedInUser);
                        else
                            Console.WriteLine("Please log in first.");
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }

                if (choice != "0")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        // ── Register ───────────────────────────────────────────────────
        static void RegisterUser(SqlData db)
        {
            Console.Clear();
            Console.WriteLine("=== REGISTER ===\n");

            Console.Write("Username    : ");
            string userName = Console.ReadLine()!;

            Console.Write("First Name  : ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name   : ");
            string lastName = Console.ReadLine()!;

            Console.Write("Password    : ");
            string password = Console.ReadLine()!;

            var user = new UserModel
            {
                UserName  = userName,
                FirstName = firstName,
                LastName  = lastName,
                Password  = password
            };

            db.RegisterUser(user);
            Console.WriteLine("\nRegistration successful!");
        }

        // ── Login ──────────────────────────────────────────────────────
        static UserModel? LoginUser(SqlData db)
        {
            Console.Clear();
            Console.WriteLine("=== LOG IN ===\n");

            Console.Write("Username : ");
            string userName = Console.ReadLine()!;

            Console.Write("Password : ");
            string password = Console.ReadLine()!;

            var user = db.GetUser(userName, password);

            if (user == null)
                Console.WriteLine("\nInvalid credentials.");
            else
                Console.WriteLine($"\nWelcome, {user.FirstName} {user.LastName}!");

            return user;
        }

        // ── List All Posts ─────────────────────────────────────────────
        static void ListAllPosts(SqlData db)
        {
            Console.Clear();
            Console.WriteLine("=== ALL POSTS ===\n");

            var posts = db.GetAllPosts();

            if (posts.Count == 0)
            {
                Console.WriteLine("No posts found.");
                return;
            }

            foreach (var post in posts)
            {
                Console.WriteLine($"[{post.Id}] {post.Title}");
                Console.WriteLine($"     By: {post.FirstName} {post.LastName} (@{post.UserName})");
                Console.WriteLine($"     Date: {post.DateCreated:yyyy-MM-dd HH:mm}");
                Console.WriteLine();
            }
        }

        // ── View Post Details ──────────────────────────────────────────
        static void ViewPostDetails(SqlData db)
        {
            Console.Clear();
            Console.WriteLine("=== POST DETAILS ===\n");

            Console.Write("Enter Post ID: ");
            if (!int.TryParse(Console.ReadLine(), out int postId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var post = db.GetPost(postId);

            if (post == null)
            {
                Console.WriteLine("Post not found.");
                return;
            }

            Console.WriteLine($"\nTitle  : {post.Title}");
            Console.WriteLine($"Author : {post.FirstName} {post.LastName} (@{post.UserName})");
            Console.WriteLine($"Date   : {post.DateCreated:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"\n{post.Body}");
        }

        // ── Create Post ────────────────────────────────────────────────
        static void CreatePost(SqlData db, UserModel user)
        {
            Console.Clear();
            Console.WriteLine("=== CREATE POST ===\n");

            Console.Write("Title : ");
            string title = Console.ReadLine()!;

            Console.Write("Body  : ");
            string body = Console.ReadLine()!;

            var post = new PostModel
            {
                UserId      = user.Id,
                Title       = title,
                Body        = body,
                DateCreated = DateTime.Now
            };

            db.CreatePost(post);
            Console.WriteLine("\nPost created successfully!");
        }
    }
}
