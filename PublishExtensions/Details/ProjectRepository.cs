using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;

namespace PHZH.PublishExtensions.Details
{
    public static class ProjectRepository
    {
        public static IList<Project> GetProjects()
        {
            List<Project> list = new List<Project>();

            foreach (Project project in Globals.Solution.Projects)
            {
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();

            foreach (ProjectItem projectIem in solutionFolder.ProjectItems)
            {
                var subProject = projectIem.SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }

            return list;
        }
    }
}