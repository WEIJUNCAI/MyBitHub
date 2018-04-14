using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

using BitHub.Data;
using BitHub.Models.Repository;

namespace BitHub.Helpers.Test
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Repositories.Any())
                return;

            var repositories = new RepositoryInfoModel[]
            {
                    new RepositoryInfoModel
                    {
                        Owner = "libgit2",
                        RepoName = "libgit2sharp",
                        Description = ".NET + git = ❤",
                        RootPath = @"/Users/jasonc/Documents/GitHub/libgit2sharp",
                        WatchCount = 124,
                        StarCount = 1390,
                        ForkCount = 536
                    }
            };

            foreach (var repo in repositories)
                context.Repositories.Add(repo);

            context.SaveChanges();

            if (context.Tags.Any())
                return;

            var tags = new RepoTagModel[]
            {
                    new RepoTagModel
                    {
                        TagName = "version-control"
                    },
                    new RepoTagModel
                    {
                        TagName = "dotnet"
                    },
                    new RepoTagModel
                    {
                        TagName = "git"
                    }
            };

            foreach (var tag in tags)
                context.Tags.Add(tag);

            context.SaveChanges();

            if (context.Tagments.Any())
                return;

            var tagments = new RepoTagmentModel[]
            {
                    new RepoTagmentModel
                    {
                        TagID = tags.Single(i => i.TagName == "git").ID,
                        RepoID = repositories.Single(i => i.RepoName == "libgit2sharp").ID
                    },
                    new RepoTagmentModel
                    {
                        TagID = tags.Single(i => i.TagName == "dotnet").ID,
                        RepoID = repositories.Single(i => i.RepoName == "libgit2sharp").ID
                    },
                    new RepoTagmentModel
                    {
                        TagID = tags.Single(i => i.TagName == "version-control").ID,
                        RepoID = repositories.Single(i => i.RepoName == "libgit2sharp").ID
                    }

            };

            foreach (var tagment in tagments)
                context.Add(tagment);

            context.SaveChanges();
        }

    }
}
