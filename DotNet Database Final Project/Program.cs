using Microsoft.EntityFrameworkCore;
using MovieLibraryEntities.Context;
using MovieLibraryEntities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MovieLibraryConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = LoggerFactory.Create(b => b.AddConsole());
            var logger = factory.CreateLogger<Program>();
            Console.WriteLine("Movie Library\n\n");
            string choice;
            do
            {
                Console.WriteLine("1) Search Movies");
                Console.WriteLine("2) Add Movie");
                Console.WriteLine("3) Update Movie");
                Console.WriteLine("");
                Console.WriteLine("4) Search Reviews");
                Console.WriteLine("5) Review Movie");
                Console.WriteLine("");
                Console.WriteLine("6) List Users");
                Console.WriteLine("7) Add User");
                Console.WriteLine("8) Update User");
                Console.WriteLine("");
                Console.WriteLine("X) Exit");
                Console.Write("Enter choice: ");
                choice = Console.ReadLine();
                Console.WriteLine("");

                switch (choice)
                {
                    case "1": // 1. Search Movies from database
                        using (var context = new MovieContext())
                        {
                            Console.Write("Search Title: ");
                            string movieSearch = Console.ReadLine().ToLower();

                            var movie = context.Movies.Include(mg => mg.MovieGenres).ThenInclude(g => g.Genre).Where(m => m.Title.ToLower().Contains(movieSearch)).ToList();
                            int movieCount = 0;
                            foreach (var mresult in movie)
                            {
                                movieCount++;
                                Console.Write($"Movie {mresult.Id}: \"{mresult.Title}\"\n\t[");
                                int commaCount = 0;
                                foreach (var gresult in mresult.MovieGenres)
                                {
                                    if (commaCount > 0)
                                    {
                                        Console.Write(",");
                                    }
                                    Console.Write($" {gresult.Genre.Name}");
                                    commaCount++;
                                }
                                Console.Write(" ]\n");
                            }
                            logger.LogInformation($"{movieCount} movies found.");
                        }
                        break;
                    case "2": // 2. Add Movie to database
                        using (var context = new MovieContext())
                        {
                            Console.Write("Enter Title: ");
                            var movieTitle = Console.ReadLine();

                            if (movieTitle != "" && movieTitle != null) // if entry is not null
                            {
                                // Create new movie
                                var movie = new Movie
                                {
                                    Title = movieTitle
                                };

                                // Save movie object to database
                                context.Movies.Add(movie);
                                context.SaveChanges();
                                logger.LogInformation($"Created Movie {movie.Id}: \"{movie.Title}\"");

                                Console.WriteLine("");
                                foreach (var gresult in context.Genres.ToList())
                                {
                                    Console.WriteLine($"Genre {gresult.Id}: \"{gresult.Name}\"");
                                }
                                bool genreLoop = true;
                                do // loop if entry is not null
                                {
                                    Console.Write("Enter Genre (Press Enter to Stop): ");
                                    var movieGenre = Console.ReadLine();

                                    var genreCheck = context.Genres.Include(mg => mg.MovieGenres).Where(g => g.Name == movieGenre).FirstOrDefault();
                                    if (genreCheck != null)
                                    {
                                        var newMovieGenre = new MovieGenre
                                        {
                                            Movie = movie,
                                            Genre = genreCheck
                                        };

                                        context.MovieGenres.Add(newMovieGenre);
                                        context.SaveChanges();
                                        logger.LogInformation($"Added Genre \"{genreCheck.Name}\" to Movie {movie.Id}");
                                    }
                                    else
                                    {
                                        genreLoop = false;
                                        logger.LogInformation("Stopped adding Movie Genres.");
                                    }
                                } while (genreLoop);
                            }
                            else
                            {
                                logger.LogInformation("Add Movie failed: Title cannot be empty.");
                            }
                        }
                        break;
                    case "3": // 3. Update Movie in database
                        using (var context = new MovieContext())
                        {
                            int movieCount = 0;
                            foreach (var mov in context.Movies)
                            {
                                movieCount++;
                            }
                            if (movieCount > 0) // if movies exist
                            {
                                Console.Write("Enter Movie ID to Edit: ");
                                var movieSearch = Console.ReadLine();

                                var movieCheck = context.Movies.Include(mg => mg.MovieGenres).ThenInclude(g => g.Genre).Where(x => x.Id == Convert.ToInt64(movieSearch)).FirstOrDefault();
                                if (movieCheck != null) // if entered movie is valid
                                {
                                    Console.Write($"Found Movie {movieCheck.Id}: \"{movieCheck.Title}\"\n\t[");
                                    int commaCount = 0;
                                    foreach (var gresult in movieCheck.MovieGenres)
                                    {
                                        if (commaCount > 0)
                                        {
                                            Console.Write(",");
                                        }
                                        Console.Write($" {gresult.Genre.Name}");
                                        commaCount++;
                                    }
                                    Console.Write(" ]\n");

                                    Console.WriteLine("1) Change Title");
                                    Console.WriteLine("2) Edit Genres");
                                    Console.WriteLine("3) Delete Movie");
                                    Console.WriteLine("Press Enter to Cancel");
                                    Console.Write("Enter choice: ");
                                    string update = Console.ReadLine();
                                    Console.WriteLine("");

                                    switch (update)
                                    {
                                        case "1": // 1. Change Title
                                            {
                                                Console.Write("Enter New Title: ");
                                                var movieTitle = Console.ReadLine();

                                                if (movieTitle != "" && movieTitle != null) // if entry is not null
                                                {
                                                    movieCheck.Title = movieTitle;
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Changed Movie {movieCheck.Id} title to \"{movieCheck.Title}\"");
                                                }
                                                else
                                                {
                                                    logger.LogInformation("Edit Movie failed, Title cannot be empty.");
                                                }
                                            }
                                            break;
                                        case "2": // 2. Edit Genres
                                            {
                                                Console.WriteLine("");
                                                foreach (var gresult in context.Genres.ToList())
                                                {
                                                    Console.WriteLine($"Genre {gresult.Id}: \"{gresult.Name}\"");
                                                }
                                                Console.WriteLine("\"Clear\": Remove all genres from movie");
                                                bool genreLoop = true;
                                                do // loop if entry is not null
                                                {
                                                    Console.Write("Enter Genre (Press Enter to Stop): ");
                                                    var movieGenre = Console.ReadLine();

                                                    var genreCheck = context.Genres.Include(mg => mg.MovieGenres).Where(g => g.Name == movieGenre).FirstOrDefault();
                                                    if (genreCheck != null) // if entered genre is valid
                                                    {
                                                        var newMovieGenre = new MovieGenre
                                                        {
                                                            Movie = movieCheck,
                                                            Genre = genreCheck
                                                        };

                                                        context.MovieGenres.Add(newMovieGenre);
                                                        context.SaveChanges();
                                                        logger.LogInformation($"Added Genre \"{genreCheck.Name}\" to Movie {movieCheck.Id}");
                                                    }
                                                    else if (movieGenre.ToLower() == "clear") // if clear is entered
                                                    {
                                                        var moviesGenres = context.MovieGenres.Include(m => m.Movie).Where(mg => mg.Movie == movieCheck).ToList();
                                                        foreach (var mresult in moviesGenres)
                                                        {
                                                            context.MovieGenres.Remove(mresult);
                                                            context.SaveChanges();
                                                            logger.LogInformation($"Removed Genre \"{mresult.Genre.Name}\" from Movie {movieCheck.Id}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        genreLoop = false;
                                                        logger.LogInformation("Stopped editing Movie Genres.");
                                                    }
                                                } while (genreLoop);
                                            }
                                            break;
                                        case "3": // 3. Delete Movie
                                            {
                                                var moviesGenres = context.MovieGenres.Include(m => m.Movie).Where(mg => mg.Movie == movieCheck).ToList();
                                                foreach (var mresult in moviesGenres) // Remove movie genres
                                                {
                                                    context.MovieGenres.Remove(mresult);
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Removed Genre \"{mresult.Genre.Name}\" from Movie {movieCheck.Id}");
                                                }

                                                var moviesReviews = context.UserMovies.Include(u => u.User).Where(um => um.Movie == movieCheck).ToList();
                                                foreach (var rresult in moviesReviews) // Remove movie ratings
                                                {
                                                    context.UserMovies.Remove(rresult);
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Removed User {rresult.User.Id} Rating from Movie {movieCheck.Id}");
                                                }

                                                context.Movies.Remove(movieCheck); // Remove movie
                                                logger.LogInformation($"Removed Movie {movieCheck.Id}");
                                                context.SaveChanges();
                                            }
                                            break;
                                        default:
                                            logger.LogInformation($"Canceled updating Movie {movieCheck.Id}");
                                            break;
                                    }
                                }
                                else
                                {
                                    logger.LogInformation($"Update Movie failed, Movie ID \"{movieSearch}\" is not valid.");
                                }
                            }
                            else
                            {
                                logger.LogInformation($"Update Movie failed, {movieCount} movies found.");
                            }
                        }
                        break;
                    case "4": // 4. Search Reviews from database
                        using (var context = new MovieContext())
                        {
                            Console.Write("Search Title: ");
                            string movieSearch = Console.ReadLine().ToLower();

                            int reviewCount = 0;
                            var movieCheck = context.Movies.Include(um => um.UserMovies).ThenInclude(u => u.User).ThenInclude(o => o.Occupation).Where(m => m.Title.ToLower().Contains(movieSearch)).OrderByDescending(m => m.UserMovies.Sum(r => r.Rating)).ThenBy(m => m.Title).FirstOrDefault(); // first sort by highest sum of ratings, then by title, and then pick first movie
                            if (movieCheck != null) // if entered movie is valid
                            {
                                Console.WriteLine($"Found Movie {movieCheck.Id}: \"{movieCheck.Title}\"");
                                var topReviews = context.UserMovies.Where(m => m.Movie == movieCheck).OrderByDescending(um => um.Rating).ToList();
                                foreach (var rresult in topReviews)
                                {
                                    reviewCount++;
                                    Console.WriteLine($"\t{reviewCount}) [ Rating {rresult.Rating} - given by User {rresult.User.Id}: [Age: {rresult.User.Age}, Gender: {rresult.User.Gender}, ZIP Code: {rresult.User.ZipCode}, Occupation: {rresult.User.Occupation.Name}] ]");
                                }
                                logger.LogInformation($"{reviewCount} reviews found for Movie {movieCheck.Id}, with ratings totalling {context.UserMovies.Where(m => m.Movie == movieCheck).Sum(r => r.Rating)}.");
                            }
                            else
                            {
                                logger.LogInformation($"Review Search failed. 0 movies found.");
                            }
                        }
                        break;
                    case "5": // 5. Rate Movie in database
                        using (var context = new MovieContext())
                        {
                            int userCount = 0;
                            int movieCount = 0;
                            foreach (var usr in context.Users)
                            {
                                userCount++;
                            }
                            foreach (var mov in context.Movies)
                            {
                                movieCount++;
                            }
                            if (userCount > 0) // if users exist
                            {
                                if (movieCount > 0) // if movies exist
                                {
                                    Console.Write("Enter User ID to Rate as: ");
                                    var userRater = Console.ReadLine();

                                    var userCheck = context.Users.Include(o => o.Occupation).Where(u => u.Id == Convert.ToInt64(userRater)).FirstOrDefault();
                                    if (userCheck != null) // if entered user is valid
                                    {
                                        Console.WriteLine($"Found User {userCheck.Id} [Age: {userCheck.Age}, Gender: {userCheck.Gender}, ZIP Code: {userCheck.ZipCode}, Occupation: {userCheck.Occupation.Name}]");
                                        Console.Write("Enter Movie ID to Rate: ");
                                        var movieRated = Console.ReadLine();

                                        var movieCheck = context.Movies.Where(x => x.Id == Convert.ToInt64(movieRated)).FirstOrDefault();
                                        if (movieCheck != null) // if entered movie is valid
                                        {
                                            Console.WriteLine($"Found Movie {movieCheck.Id}: \"{movieCheck.Title}\"");

                                            var reviewExists = context.UserMovies.Where(m => m.Movie == movieCheck).Where(u => u.User == userCheck).FirstOrDefault();
                                            if (reviewExists != null) // if user has already rated movie
                                            {
                                                Console.WriteLine($"User {userCheck.Id} has already given Movie {movieCheck.Id} a Rating of \"{reviewExists.Rating}\"");
                                            }
                                            Console.Write("Enter Rating (1-5): ");
                                            var movieRating = Console.ReadLine();

                                            if (movieRating != null && int.TryParse(movieRating, out int rnumber) && rnumber >= 1 && rnumber <= 5) // if entry is integer and not null and between 1 and 5
                                            {
                                                if (reviewExists == null) // add rating if user ha not already rated movie
                                                {
                                                    // Create new rating
                                                    var usermovie = new UserMovie
                                                    {
                                                        Rating = Convert.ToInt64(movieRating),
                                                        User = userCheck,
                                                        Movie = movieCheck,
                                                        RatedAt = DateTime.Now
                                                    };

                                                    // Save user object to database
                                                    context.UserMovies.Add(usermovie);
                                                    context.SaveChanges();
                                                    logger.LogInformation($"User {userCheck.Id} gave Movie {movieCheck.Id} a Rating of \"{usermovie.Rating}\"");
                                                }
                                                else // replace rating if user has already rated movie
                                                {
                                                    reviewExists.Rating = Convert.ToInt64(movieRating);
                                                    reviewExists.RatedAt = DateTime.Now;
                                                    context.SaveChanges();
                                                    logger.LogInformation($"User {userCheck.Id} changed Movie {movieCheck.Id} Rating to \"{reviewExists.Rating}\"");
                                                }
                                            }
                                            else
                                            {
                                                logger.LogInformation($"Review Movie failed, Rating \"{movieRating}\" is not valid.");
                                            }
                                        }
                                        else
                                        {
                                            logger.LogInformation($"Review Movie failed, Movie ID \"{movieRated}\" is not valid.");
                                        }
                                    }
                                    else
                                    {
                                        logger.LogInformation($"Review Movie failed, User ID \"{userRater}\" is not valid.");
                                    }
                                }
                                else
                                {
                                    logger.LogInformation($"Review Movie failed, {movieCount} movies found.");
                                }
                            }
                            else
                            {
                                logger.LogInformation($"Review Movie failed, {userCount} users found.");
                            }
                        }
                        break;
                    case "6": // 6. List Users from database
                        using (var context = new MovieContext())
                        {
                            var user = context.Users.Include(o => o.Occupation).OrderBy(u => u.Id).ToList();
                            int userCount = 0;
                            foreach (var u in user)
                            {
                                System.Console.WriteLine($"User {u.Id}: [Age: {u.Age}, Gender: {u.Gender}, ZIP Code: {u.ZipCode}, Occupation: {u.Occupation.Name}]");
                                userCount++;
                            }
                            logger.LogInformation($"{userCount} users found.");
                        }
                        break;
                    case "7": // 7. Add User to database
                        using (var context = new MovieContext())
                        {
                            Console.Write("Enter User Age: ");
                            var userAge = Console.ReadLine();

                            if (userAge != null && int.TryParse(userAge, out int agenumber)) // if entry is integer and not null
                            {
                                Console.Write("Enter User Gender (M/F): ");
                                var userGender = Console.ReadLine();

                                if (userGender != null && (userGender.ToUpper() == "M" || userGender.ToUpper() == "F")) // if entry not null and m or f
                                {
                                    Console.Write("Enter User ZIP Code: ");
                                    var userZipcode = Console.ReadLine();

                                    if (userZipcode != null && userZipcode != "") // if entry is not null
                                    {
                                        Console.WriteLine("");
                                        foreach (var oresult in context.Occupations.ToList())
                                        {
                                            Console.WriteLine($"Occupation {oresult.Id}: \"{oresult.Name}\"");
                                        }
                                        Console.Write("Select Occupation name: ");
                                        var userOccupation = Console.ReadLine();

                                        if (userOccupation == null || userOccupation == "") // if entry is null then set to 'none' haha
                                        {
                                            userOccupation = "None";
                                        }
                                        var occupationCheck = context.Occupations.Where(o => o.Name == userOccupation).FirstOrDefault();
                                        if (occupationCheck != null) // check if entered occupation is valid
                                        {
                                            // Create new user
                                            var user = new User
                                            {
                                                Age = Convert.ToInt64(userAge),
                                                Gender = userGender.ToUpper(),
                                                ZipCode = userZipcode.ToUpper(),
                                                Occupation = occupationCheck
                                            };

                                            // Save user object to database
                                            context.Users.Add(user);
                                            context.SaveChanges();
                                            logger.LogInformation($"Created User {user.Id}: [Age: {user.Age}, Gender: {user.Gender}, ZIP Code: {user.ZipCode}, Occupation: {user.Occupation.Name}]");
                                        }
                                        else
                                        {
                                            logger.LogInformation($"Add User failed, Occupation \"{userOccupation}\" is not valid.");
                                        }
                                    }
                                    else
                                    {
                                        logger.LogInformation($"Add User failed, ZIP Code \"{userZipcode}\" is not valid.");
                                    }
                                }
                                else
                                {
                                    logger.LogInformation($"Add User failed, Gender \"{userGender}\" is not valid. What are you gonna do, cancel me?"); // funny
                                }
                            }
                            else
                            {
                                logger.LogInformation($"Add User failed, Age \"{userAge}\" is not valid.");
                            }
                        }
                        break;
                    case "8": // 8. Update User in database
                        using (var context = new MovieContext())
                        {
                            int userCount = 0;
                            foreach (var mov in context.Users)
                            {
                                userCount++;
                            }
                            if (userCount > 0) // if users exist
                            {
                                Console.Write("Enter User ID to Edit: ");
                                var userSearch = Console.ReadLine();

                                var userCheck = context.Users.Include(o => o.Occupation).Where(u => u.Id == Convert.ToInt64(userSearch)).FirstOrDefault();
                                if (userCheck != null) // if entered user is valid
                                {
                                    Console.WriteLine($"Found User {userCheck.Id} [Age: {userCheck.Age}, Gender: {userCheck.Gender}, ZIP Code: {userCheck.ZipCode}, Occupation: {userCheck.Occupation.Name}]");

                                    Console.WriteLine("1) Change Age");
                                    Console.WriteLine("2) Change Gender");
                                    Console.WriteLine("3) Change ZIP Code");
                                    Console.WriteLine("4) Change Occupation");
                                    Console.WriteLine("5) Delete User");
                                    Console.WriteLine("Press Enter to Cancel");
                                    Console.Write("Enter choice: ");
                                    string update = Console.ReadLine();
                                    Console.WriteLine("");

                                    switch (update)
                                    {
                                        case "1": // 1. Change Age
                                            {
                                                Console.Write("Enter new User Age: ");
                                                var userAge = Console.ReadLine();

                                                if (userAge != null && int.TryParse(userAge, out int agenumber)) // if entry is integer and not null
                                                {
                                                    userCheck.Age = Convert.ToInt64(userAge);
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Changed User {userCheck.Id} Age to {userCheck.Age}");
                                                }
                                                else
                                                {
                                                    logger.LogInformation($"Edit User failed, Age \"{userAge}\" is not valid.");
                                                }
                                            }
                                            break;
                                        case "2": // 2. Change Gender
                                            {
                                                Console.Write("Enter new User Gender (M/F): ");
                                                var userGender = Console.ReadLine();

                                                if (userGender != null && (userGender.ToUpper() == "M" || userGender.ToUpper() == "F")) // if entry not null and m or f
                                                {
                                                    userCheck.Gender = userGender.ToUpper();
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Changed User {userCheck.Id} Gender to {userCheck.Gender}. Good for them.");
                                                }
                                                else
                                                {
                                                    logger.LogInformation($"Edit User failed, Gender \"{userGender}\" is not valid. What are you gonna do, cancel me?");
                                                }
                                            }
                                            break;
                                        case "3": // 3. Change ZIP Code
                                            {
                                                Console.Write("Enter new User ZIP Code: ");
                                                var userZipcode = Console.ReadLine();

                                                if (userZipcode != null && userZipcode != "") // if entry is not null
                                                {
                                                    userCheck.ZipCode = userZipcode.ToUpper();
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Changed User {userCheck.Id} ZIP Code to \"{userCheck.ZipCode}\"");
                                                }
                                                else
                                                {
                                                    logger.LogInformation($"Edit User failed, ZIP Code \"{userZipcode}\" is not valid.");
                                                }
                                            }
                                            break;
                                        case "4": // 4. Change Occupation
                                            {
                                                Console.WriteLine("");
                                                foreach (var oresult in context.Occupations.ToList())
                                                {
                                                    Console.WriteLine($"Occupation {oresult.Id}: \"{oresult.Name}\"");
                                                }
                                                Console.Write("Select new Occupation name: ");
                                                var userOccupation = Console.ReadLine();

                                                if (userOccupation == null || userOccupation == "") // if entry is null then set to 'none' haha
                                                {
                                                    userOccupation = "None";
                                                }
                                                var occupationCheck = context.Occupations.Where(o => o.Name == userOccupation).FirstOrDefault();
                                                if (occupationCheck != null) // check if entered occupation is valid
                                                {
                                                    userCheck.Occupation = occupationCheck;
                                                    context.SaveChanges();
                                                    logger.LogInformation($"Changed User {userCheck.Id} Occupation to \"{userCheck.Occupation.Name}\"");
                                                }
                                                else
                                                {
                                                    logger.LogInformation($"Edit User failed, Occupation \"{userOccupation}\" is not valid.");
                                                }
                                            }
                                            break;
                                        case "5": // 5. Delete User
                                            {
                                                var userReviews = context.UserMovies.Include(u => u.User).Where(um => um.User == userCheck).ToList();
                                                foreach (var rresult in userReviews)
                                                {
                                                    context.UserMovies.Remove(rresult);
                                                    logger.LogInformation($"Removed User {userCheck.Id} Rating from Movie {rresult.Movie.Id}");
                                                    context.SaveChanges();
                                                }
                                                context.Users.Remove(userCheck);
                                                logger.LogInformation($"Removed User {userCheck.Id}. They will be dearly missed.");
                                                context.SaveChanges();
                                            }
                                            break;
                                        default:
                                            logger.LogInformation($"Canceled editing User {userCheck.Id}");
                                            break;
                                    }
                                }
                                else
                                {
                                    logger.LogInformation($"Update User failed, User ID \"{userSearch}\" is not valid.");
                                }
                            }
                            else
                            {
                                logger.LogInformation($"Update User failed, {userCount} users found.");
                            }
                        }
                        break;
                    default:
                        break;
                }
                Console.WriteLine("");
            } while (choice != null && choice.ToLower() != "x");
        }
    }
}