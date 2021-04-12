using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace upject
{
    class Program
    {
        const string projectsIndent = "# ";
        const string columnIndent = "## ";
        const string cardsIndent = "### ";

        static Program() {
            string login = Environment.GetEnvironmentVariable("UPJECT_USERNAME");
            string password = Environment.GetEnvironmentVariable("UPJECT_API_KEY");

            if (String.IsNullOrEmpty(password)) {
                password = Environment.GetEnvironmentVariable("UPJECT_PASSWORD");
            }

            bool isValid = true;

            if (String.IsNullOrEmpty(login)) {
                if (!Console.IsOutputRedirected) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not get username from variable UPJECT_USERNAME. Are you sure you set it ?");
                }

                isValid = false;
            }

            if (String.IsNullOrEmpty(password)) {
                if (!Console.IsOutputRedirected) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not get password/API key from variable UPJECT_PASSWORD or UPJECT_API_KEY. Are you sure you set it ?");
                }

                isValid = false;
            }

            if (!isValid) {
                Environment.Exit(1);
            }

            baseClient = new GitHubClient(new ProductHeaderValue("upject")) {
                Credentials = new Credentials(login, password)
            };

            projectsClient = baseClient.Repository.Project;
            columnsClient  = projectsClient.Column;
            cardsClient    = projectsClient.Card;
            issuesClient   = baseClient.Issue;
        }

        static readonly GitHubClient baseClient;

        static readonly IProjectsClient projectsClient;

        static readonly IProjectColumnsClient columnsClient;

        static readonly IProjectCardsClient cardsClient;

        static readonly IIssuesClient issuesClient;

        static Repository repo;

        static TextWriter writer;

        static void Main(string[] args)
        {
            string userName, repoName;

            if (args is null || args.Length < 1) {

                PrintHelp();

                return;
            }

            if (args.Length == 1) {
                var split = args[0].Split('/');

                if (split.Length < 2) {
                    PrintHelp();

                    return;
                }

                userName = split[0];
                repoName = split[1];
            } else {
                userName = args[0];
                repoName = args[1];
            }

            try {
                repo = baseClient.Repository.Get(userName, repoName).Result;
            } catch (AggregateException ae) when (ae.InnerException is NotFoundException) {
                Console.Error.WriteLine("ERROR: Could not find repository " + repoName + " for user " + userName);
                Console.Error.WriteLine();
                PrintHelp();
                return;
            } catch (AggregateException ae) when (ae.InnerException is System.Net.Http.HttpRequestException e) {
                Console.Error.WriteLine("ERROR: Could not connect to github's API (either Internet is too slow or the connection was blocked.)");
                return;
            }

            //TextWriter writer;

            if (!Console.IsErrorRedirected) {
                var file = new FileInfo(repoName + "_projects.md");

                Console.Error.WriteLine("Writing to " + file.FullName);

                writer = new StreamWriter(file.FullName);
            } else if (Console.IsOutputRedirected) {
                Console.Error.WriteLine("It is not recommended to use cli redirects, as it will erase the previous file even if it fails.");
                writer = Console.Out;
            }

            writer = TextWriter.Synchronized(writer);

            if (!Console.IsErrorRedirected) {
                PrintRateLimitInfo(baseClient.GetLastApiInfo().RateLimit);
            }

            var projects = projectsClient.GetAllForRepository(repo.Id).Result;

            Parallel.ForEach(projects, project => {
                WriteProject(project);
            });

            writer.Dispose();
        }

        static void PrintRateLimitInfo(RateLimit limit) {

            var percentLeft = 100 * limit.Remaining / limit.Limit;

            if (percentLeft <= 25) {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (percentLeft <= 50) {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.Write($"{limit.Remaining} ({percentLeft}%)");
            Console.ResetColor();

            Console.Write(" API calls remaining out of " + limit.Limit + ", resetting on ");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(limit.Reset.ToLocalTime().ToString("f"));
            Console.ResetColor();

        }

        static void WriteProject(Project project) {
            writer.WriteLine(projectsIndent + project.Name + writer.NewLine);

            Parallel.ForEach(columnsClient.GetAll(project.Id).Result, column => {
                WriteColumn(column);
            });

            writer.WriteLine(writer.NewLine);
        }

        static void WriteColumn(ProjectColumn column) {
            writer.WriteLine(columnIndent + column.Name + writer.NewLine);
            writer.WriteLine(writer.NewLine + "---" + writer.NewLine);

            Parallel.ForEach(cardsClient.GetAll(column.Id).Result, card => {
                WriteCard(card);
            });

            writer.WriteLine();
        }

        static void WriteCard(ProjectCard card) {
            string text;


            if (card.ContentUrl != null) {
                var cardURL = card.ContentUrl;
                var issueID = Int32.Parse(cardURL.Split('/').Last());
                var issue = issuesClient.Get(repo.Id, issueID).Result;

                text = MakeLink(issue.Title, issue.Url);

                if (issue.State.Value == ItemState.Closed) {
                    text = MakeStrikethrough(text);
                }

                text = MakeItalic("#" + issueID) + " -- " + text;
            } else {
                text = card.Note;
            }


            writer.WriteLine(text);
            writer.WriteLine(writer.NewLine + "---" + writer.NewLine);
        }

        static void PrintHelp()
            =>  Console.Error.WriteLine(
                    "Usage : upject [user] [repo]" + Console.Out.NewLine
                  + "        upject \"[user]/[repo]\" (i.e. a github url's last part, and you can ignore the quotes if it doesn't match any file/directory)"
                );

        static string MakeLink(string text, string link) => $"[{text}]({link})";

        static string MakeItalic(string text) => '*' + text + '*';

        static string MakeStrikethrough(string text) => "~~" + text + "~~";
    }
}
