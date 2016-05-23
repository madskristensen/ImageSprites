using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace ImageSpritesVsix
{
    public static class ProjectHelpers
    {
        static ProjectHelpers()
        {
            DTE = (DTE2)Package.GetGlobalService(typeof(DTE));
        }

        public static DTE2 DTE { get; }

        public static void CheckFileOutOfSourceControl(string file)
        {
            if (!File.Exists(file) || DTE.Solution.FindProjectItem(file) == null)
                return;

            if (DTE.SourceControl.IsItemUnderSCC(file) && !DTE.SourceControl.IsItemCheckedOut(file))
                DTE.SourceControl.CheckOutItem(file);

            FileInfo info = new FileInfo(file);
            info.IsReadOnly = false;
        }

        internal static void ExecuteCommand(string name)
        {
            var command = DTE.Commands.Item(name);
            if (command.IsAvailable)
                DTE.ExecuteCommand(name);
        }

        public static IEnumerable<ProjectItem> GetSelectedItems()
        {
            var items = (Array)DTE.ToolWindows.SolutionExplorer.SelectedItems;

            foreach (UIHierarchyItem selItem in items)
            {
                ProjectItem item = selItem.Object as ProjectItem;

                if (item != null)
                    yield return item;
            }
        }

        public static bool GetProjectOrSolution(out Project project, out Solution2 solution)
        {
            var items = (Array)DTE.ToolWindows.SolutionExplorer.SelectedItems;
            project = null;
            solution = null;

            foreach (UIHierarchyItem selItem in items)
            {
                var projItem = selItem.Object as Project;

                if (projItem != null)
                    project = projItem;

                var solItem = selItem.Object as Solution2;

                if (solItem != null)
                    solution = solItem;
            }

            return project != null || solution != null;
        }

        public static IEnumerable<string> GetSelectedItemPaths()
        {
            foreach (ProjectItem item in GetSelectedItems())
            {
                if (item != null && item.Properties != null)
                    yield return item.Properties.Item("FullPath").Value.ToString();
            }
        }

        public static string GetRootFolder(this Project project)
        {
            if (project == null || string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;

            if (Directory.Exists(fullPath))
                return fullPath;

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath);

            return null;
        }

        public static void AddFileToProject(this Project project, string file, string itemType = null)
        {
            if (project.IsKind(ProjectTypes.ASPNET_5))
                return;

            if (DTE.Solution.FindProjectItem(file) == null)
            {
                ProjectItem item = project.ProjectItems.AddFromFile(file);
                item.SetItemType(itemType);
            }
        }

        public static void SetItemType(this ProjectItem item, string itemType)
        {
            try
            {
                if (item == null || item.ContainingProject == null)
                    return;

                if (string.IsNullOrEmpty(itemType)
                    || item.ContainingProject.IsKind(ProjectTypes.WEBSITE_PROJECT)
                    || item.ContainingProject.IsKind(ProjectTypes.UNIVERSAL_APP))
                    return;

                item.Properties.Item("ItemType").Value = itemType;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static void AddNestedFile(string parentFile, string newFile, string itemType = null)
        {
            ProjectItem item = DTE.Solution.FindProjectItem(parentFile);

            try
            {
                if (item == null
                    || item.ContainingProject == null
                    || item.ContainingProject.IsKind(ProjectTypes.ASPNET_5))
                    return;

                if (item.ProjectItems == null || item.ContainingProject.IsKind(ProjectTypes.UNIVERSAL_APP))
                {
                    item.ContainingProject.AddFileToProject(newFile);
                }
                else if (DTE.Solution.FindProjectItem(newFile) == null)
                {
                    item.ProjectItems.AddFromFile(newFile);
                }

                ProjectItem newItem = DTE.Solution.FindProjectItem(newFile);
                newItem.SetItemType(itemType);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static bool IsKind(this Project project, string kindGuid)
        {
            return project.Kind.Equals(kindGuid, StringComparison.OrdinalIgnoreCase);
        }

        public static void DeleteFileFromProject(string file)
        {
            ProjectItem item = DTE.Solution.FindProjectItem(file);

            if (item == null)
                return;
            try
            {
                item.Delete();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static Project GetActiveProject()
        {
            try
            {
                Window2 window = DTE.ActiveWindow as Window2;
                Document doc = DTE.ActiveDocument;

                if (window != null && window.Type == vsWindowType.vsWindowTypeDocument)
                {
                    // if a document is active, use the document's containing directory
                    if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                    {
                        ProjectItem docItem = DTE.Solution.FindProjectItem(doc.FullName);

                        if (docItem != null && docItem.ContainingProject != null)
                            return docItem.ContainingProject;
                    }
                }

                Array activeSolutionProjects = DTE.ActiveSolutionProjects as Array;

                if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                    return activeSolutionProjects.GetValue(0) as Project;

                if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                {
                    var item = DTE.Solution?.FindProjectItem(doc.FullName);

                    if (item != null)
                        return item.ContainingProject;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error getting the active project" + ex);
            }

            return null;
        }

        public static void SelectInSolutionExplorer(string selected)
        {
            UIHierarchy solutionExplorer = (UIHierarchy)DTE.Windows.Item(EnvDTE.Constants.vsext_wk_SProjectWindow).Object;
            UIHierarchyItem rootNode = solutionExplorer.UIHierarchyItems.Item(1);

            Stack<Tuple<UIHierarchyItems, int, bool>> parents = new Stack<Tuple<UIHierarchyItems, int, bool>>();
            ProjectItem targetItem = DTE.Solution.FindProjectItem(selected);

            if (targetItem == null)
            {
                return;
            }

            UIHierarchyItems collection = rootNode.UIHierarchyItems;
            int cursor = 1;
            bool oldExpand = collection.Expanded;

            while (cursor <= collection.Count || parents.Count > 0)
            {
                while (cursor > collection.Count && parents.Count > 0)
                {
                    collection.Expanded = oldExpand;
                    Tuple<UIHierarchyItems, int, bool> parent = parents.Pop();
                    collection = parent.Item1;
                    cursor = parent.Item2;
                    oldExpand = parent.Item3;
                }

                if (cursor > collection.Count)
                {
                    break;
                }

                UIHierarchyItem result = collection.Item(cursor);
                ProjectItem item = result.Object as ProjectItem;

                if (item == targetItem)
                {
                    result.Select(vsUISelectionType.vsUISelectionTypeSelect);
                    return;
                }

                ++cursor;

                bool oldOldExpand = oldExpand;
                oldExpand = result.UIHierarchyItems.Expanded;
                result.UIHierarchyItems.Expanded = true;
                if (result.UIHierarchyItems.Count > 0)
                {
                    parents.Push(Tuple.Create(collection, cursor, oldOldExpand));
                    collection = result.UIHierarchyItems;
                    cursor = 1;
                }
            }
        }

        public static class ProjectTypes
        {
            public const string ASPNET_5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
            public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
            public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
            public const string NODE_JS = "{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
        }
    }
}