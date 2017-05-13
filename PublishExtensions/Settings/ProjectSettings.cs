using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EnvDTE;
using PHZH.PublishExtensions.Details;

namespace PHZH.PublishExtensions.Settings
{
    [Serializable]
    [XmlRoot("ProjectSettings")]
    public class ProjectSettings : ConfigBase<ProjectSettings>
    {
        public const bool DEFAULT_MAPPING_ENABLED = false;
        public const string DEFAULT_IGNORE_FILTER = "*.config; *.cs; *.csproj; *.vsdoc.js; *.bundle; *.sln; *.vssscc; *.vspscc; *.mcwebhelp; *.snk; *.tdf; *.cd; *.settings; *.pubxml; *.map; *.wsdl; *.datasource; *.disco; *.tt; *.t4; T4MVC*; *.user; *.resx; _references.js; obj\\; Properties\\; _bin_deployableAssemblies\\; App_Data\\;";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectSettings"/> class.
        /// </summary>
        public ProjectSettings()
        {
            PublishTarget = new PublishTarget();
            IgnoreFilter = DEFAULT_IGNORE_FILTER;
            MappingEnabled = DEFAULT_MAPPING_ENABLED;
            Items = new ItemCollection();
            Assemblies = string.Empty;
            AssemblyPublishFolder = string.Empty;
        }

        [XmlElement("PublishTarget")]
        public PublishTarget PublishTarget { get; set; }

        [XmlElement("IgnoreFilter")]
        public string IgnoreFilter { get; set; }

        [XmlElement("MappingEnabled")]
        public bool MappingEnabled { get; set; }

        [XmlElement("Items")]
        public ItemCollection Items { get; set; }

        [XmlElement("Assemblies")]
        public string Assemblies { get; set; }

        [XmlElement("AssemblyPublishFolder")]
        public string AssemblyPublishFolder { get; set; }

        public void UpdatePublishTarget(bool isUserSpecific, string location)
        {
            PublishTarget.IsUserSpecific = isUserSpecific;
            if (!isUserSpecific)
            {
                PublishTarget.Location = location;
            }
            else
            {
                UserLocation loc = PublishTarget.UserLocations.FirstOrDefault(l => l.UserName == Globals.UserName);
                if (loc == null)
                {
                    loc = new UserLocation { UserName = Globals.UserName };
                    PublishTarget.UserLocations.Add(loc);
                }

                loc.Location = location;
            }
        }

        public string GetPublishLocation()
        {
            if (PublishTarget.IsUserSpecific)
            {
                UserLocation loc = PublishTarget.UserLocations.FirstOrDefault(l => l.UserName == Globals.UserName);
                return loc != null ? loc.Location : null;
            }
            else
                return PublishTarget.Location.OrNull();
        }

        public bool ItemRemoved(ProjectItem item)
        {
            if (item == null)
                return false;

            string path = item.GetRelativePath();

            Logger.Log("Item removed: {0}", path);

            // is folder?
            if (item.Kind == Constants.vsProjectItemKindPhysicalFolder)
                return RemoveFolder(path);
            else
                return Items.Remove(path);
        }

        private bool RemoveFolder(string path)
        {
            List<ItemSettings> folderItems = Items.GetItemsStartingWith(path);
            if (folderItems.Count() < 1)
                return false;

            Logger.Log(" -> Removing {0} item setting(s)", folderItems.Count());

            foreach (var item in folderItems)
                Items.Remove(item);

            return true;
        }

        public bool ItemRenamed(ProjectItem item, string oldFullPath)
        {
            bool settingsModified = false;
            if (item == null)
                return settingsModified;
        
            bool isFolder = item.Kind == Constants.vsProjectItemKindPhysicalFolder;
            string newRelativePath = item.GetRelativePath();

            // get path of item
            // - file  : path\to\file.ext
            // - folder: path\to\folder\ 
            string itemPath = newRelativePath.SubstringBeforeLast("\\");
            if (isFolder)
                itemPath = itemPath.SubstringBeforeLast("\\");

            string oldRelativePath = oldFullPath.Replace(item.ContainingProject.GetDirectory(), "");
            if (isFolder)
                oldRelativePath = oldRelativePath.EnsureEndingDirectorySeparator();

            // not a folder?
            if (!isFolder)
            {
                Logger.Log("Item renamed: {0} => {1}", oldRelativePath, newRelativePath);
                ItemSettings settings = Items.TryGet(oldRelativePath);
                if (settings != null)
                {
                    Items.Rename(settings, newRelativePath);
                    settingsModified = true;
                }
            }
            else
            {
                Logger.Log("Folder renamed: {0} => {1}", oldRelativePath, newRelativePath);
                settingsModified = RenameFolder(newRelativePath, oldRelativePath);
            }

            return settingsModified;
        }

        private bool RenameFolder(string newPath, string oldPath)
        {
            // get all items
            List<ItemSettings> folderItems = Items.GetItemsStartingWith(oldPath);
            if (folderItems.Count() < 1)
                return false;

            Logger.Log(" -> Renaming {0} item setting(s)", folderItems.Count());

            // remove all to avoid path conflicts
            foreach (var item in folderItems)
                Items.Remove(item);

            // change path for each and add it
            foreach (var item in folderItems)
            {
                string path = newPath + item.Path.Remove(0, oldPath.Length);
                Items.Add(new ItemSettings(path, item));
            }

            return true;
        }

        public bool AddItemSettings(ItemSettings item)
        {
            return Items.Add(item);
        }
        
        public ItemSettings GetItemSettings(string relativePath)
        {
            return Items.TryGet(relativePath) ?? new ItemSettings(relativePath);
        }

        public string GetItemMapping(string relativePath)
        {
            ItemSettings item = Items.TryGet(relativePath);
            return item != null ? item.Mapping : ItemSettings.MappingDefault;
        }

        public void SetItemMapping(string relativePath, string mapping)
        {
            ItemSettings item = Items.TryGet(relativePath);
            if (item == null)
            {
                item = new ItemSettings(relativePath);
                Items.Add(item);
            }
            item.Mapping = mapping;
        }

        public bool IsItemIgnored(string relativePath)
        {
            ItemSettings item = Items.TryGet(relativePath);
            return item != null ? item.Ignore : ItemSettings.IgnoreDefault;
        }

        public void IgnoreItem(string relativePath, bool ignore)
        {
            ItemSettings item = Items.TryGet(relativePath);
            if (item == null)
            {
                item = new ItemSettings(relativePath);
                Items.Add(item);
            }
            item.Ignore = ignore;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public override void Save()
        {
            Globals.CheckOutFileFromSCC(this.FilePath);
            base.Save();
        }

        /// <summary>
        /// Called after the configuration was deserialized.
        /// </summary>
        protected override void OnDeserialized()
        {
            if (PublishTarget == null)
                PublishTarget = new PublishTarget();

            if (PublishTarget.UserLocations == null)
                PublishTarget.UserLocations = new List<UserLocation>();
        }
    }
}
