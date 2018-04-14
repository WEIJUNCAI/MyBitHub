using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Helpers.Repository
{
    public static class GitHelper
    {

        public static void CheckoutBranch(this IRepository repository, string branch)
        {
            try
            {
                Branch targetBranch = repository.Branches[branch];
                Commands.Checkout(repository, targetBranch);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed checking out branch \"{branch}\".", ex);
            }
        }


        // get the latest commit associated with a folder or file
        //  should have been implemented with libgit2sharp, but
        //  the "QueryBy(file)" method apparently has serious bug
        //  so we have to use command line git
        public static IEnumerable<Commit> GetLatestCommits(this IRepository repository,
            string repoRootPath, IEnumerable<string> targetRelativePaths, bool includeRename)
        {
            List<Commit> commits = new List<Commit>();
            try
            {
                var shas = GetLatestCommitShas(repoRootPath, targetRelativePaths, includeRename);

                foreach (string sha in shas)
                    commits.Add((sha.Length == 0) ? null : repository.Lookup<Commit>(sha));

                return commits;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed retrieving latest commit from command line.", ex);
            }
        }

        // helper method that gets the SHA of the desired latest commit associated with a file or folder
        // use command line git and parse the output

        // helper method that gets the SHAs of the desired latest commit associated with all files and folders
        // we provide it with all target paths to avoid the overhead of creating a new porocess
        // for each path.
        // use command line git and parse the output
        // note the use of ReadLine or ReadToEnd might block indefinitely since we do not provide starting arguments
        // and in this case the output stream does not have an end

        // we will sure find the command that has been echoed back after executing it
        // and on the next line, the git log result.
        // if it is empty, no commit record for this path; otheriwse we get the commit SHA
        // *** Note *** 
        // although very unlikely, the description might also contain the same command
        // which could lead to misinterpretation.

        private static IEnumerable<string> GetLatestCommitShas(string repoRootPath, IEnumerable<string> targetRelativePaths, bool includeRename)
        {
            List<string> commitShas = new List<string>();

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "/bin/bash",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = repoRootPath
            };

            string commandOption = includeRename ? "--follow" : string.Empty;
            process.StartInfo = startInfo;
            process.Start();

            foreach (string path in targetRelativePaths)
            {
                string command = $"git log -n 1 {commandOption} \"{path}\"\n";
                process.StandardInput.Write(command);

                while (true)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line.IndexOf("commit ") == 0)
                    {
                        int index1 = line.IndexOf(' ');
                        if (index1 != -1)
                            line = line.Substring(index1 + 1, line.Length - index1 - 1);
                        commitShas.Add(line);
                        break;
                    }
                }
            }
            process.Close();

            return commitShas;
        }
    }
}
