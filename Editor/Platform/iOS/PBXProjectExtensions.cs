#if ((UNITY_TVOS || UNITY_STANDALONE_OSX || UNITY_VISIONOS) && ENABLE_LOCALIZATION_XCODE_SUPPORT) || (UNITY_IOS || UNITY_IPHONE)
using System;
using System.Collections;
using System.Reflection;
using UnityEditor.iOS.Xcode;
using Debug = UnityEngine.Debug;

namespace UnityEditor.Localization.Platform.iOS
{
    /// <summary>
    /// PBXProject helpers that reach into internal members via reflection.
    /// <see cref="RemoveLocaleFromVariantGroup"/> and <see cref="IncludeVariantGroupInBuild"/>
    /// are always available; the four region/locale extension methods at the bottom
    /// became public on <see cref="PBXProject"/> in Unity 2023.1 and are gated to
    /// older editors only.
    /// </summary>
    static class PBXProjectExtensions
    {
        readonly static Type s_GUIDList;
        readonly static Type s_PBXVariantGroupData;
        readonly static FieldInfo s_ProjectData;
        readonly static FieldInfo s_PBXObjectGuid;
        readonly static FieldInfo s_GroupChildren;
        readonly static FieldInfo s_VariantGroupName;
        readonly static FieldInfo s_FileRefName;
        readonly static FieldInfo s_DataFileRefsField;
        readonly static PropertyInfo s_ProjectVariantGroups;
        readonly static PropertyInfo s_FileRefsIndexer;
        readonly static MethodInfo s_VariantGroupsObjects;
        readonly static MethodInfo s_GUIDListContains;
        readonly static MethodInfo s_GuidListAdd;
        readonly static MethodInfo s_GuidListRemove;
        readonly static MethodInfo s_FileRefsRemoveEntry;
        readonly static MethodInfo s_ProjectBuildFilesGetForSourceFile;
        readonly static MethodInfo s_GroupsGetMainGroup;

        #if !UNITY_2023_1_OR_NEWER
        readonly static Type s_PBXBuildFileDat;
        readonly static Type s_PBXElementArray;
        readonly static Type s_PBXElementString;
        readonly static FieldInfo s_DataFileGroups;
        readonly static FieldInfo s_KnownRegionsDict;
        readonly static FieldInfo s_GroupName;
        readonly static FieldInfo s_GroupPath;
        readonly static FieldInfo s_PBXElementArrayValues;
        readonly static FieldInfo s_ResourceFiles;
        readonly static PropertyInfo s_FileRefsPath;
        readonly static PropertyInfo s_ProjectResoruces;
        readonly static PropertyInfo s_ProjectSection;
        readonly static PropertyInfo s_ProjectSectionObjectData;
        readonly static MethodInfo s_DataFileRefsFieldObjects;
        readonly static MethodInfo s_FileRefDataCreateFromFile;
        readonly static MethodInfo s_GetPropertiesRaw;
        readonly static MethodInfo s_GroupsObjects;
        readonly static MethodInfo s_PBXBuildFileDataCreateFromFile;
        readonly static MethodInfo s_ProjectBuildFilesAdd;
        readonly static MethodInfo s_ProjectFileRefsAdd;
        readonly static MethodInfo s_RawPropertiesValuesAddValue;
        readonly static MethodInfo s_RawPropertiesValuesGetValue;
        readonly static MethodInfo s_ResorucesObjects;
        readonly static MethodInfo s_VariantGroupsAddEntry;
        readonly static MethodInfo s_VariantGroupsSetPropertyString;
        #endif

        static PBXProjectExtensions()
        {
            var asm = typeof(PBXProject).Assembly;
            const string ns = "UnityEditor.iOS.Xcode.PBX";
            const BindingFlags pv = BindingFlags.Instance | BindingFlags.NonPublic;

            s_GUIDList = asm.GetType($"{ns}.GUIDList");
            s_PBXVariantGroupData = asm.GetType($"{ns}.PBXVariantGroupData");
            var fileRefData = asm.GetType($"{ns}.PBXFileReferenceData");
            var pbxObject = asm.GetType($"{ns}.PBXObjectData");
            var group = asm.GetType($"{ns}.PBXGroupData");

            s_ProjectData = typeof(PBXProject).GetField("m_Data", pv);
            s_PBXObjectGuid = pbxObject.GetField("guid");
            s_GroupChildren = group.GetField("children");
            s_VariantGroupName = s_PBXVariantGroupData.GetField("name");
            s_FileRefName = fileRefData.GetField("name");
            s_DataFileRefsField = s_ProjectData.FieldType.GetField("fileRefs", pv);
            s_ProjectVariantGroups = typeof(PBXProject).GetProperty("variantGroups", pv);
            s_FileRefsIndexer = s_DataFileRefsField.FieldType.GetProperty("Item", new[] { typeof(string) });
            s_VariantGroupsObjects = s_ProjectVariantGroups.PropertyType.GetMethod("GetObjects");
            s_GUIDListContains = s_GUIDList.GetMethod("Contains");
            s_GuidListAdd = s_GUIDList.GetMethod("AddGUID");
            s_GuidListRemove = s_GUIDList.GetMethod("RemoveGUID");
            s_FileRefsRemoveEntry = s_DataFileRefsField.FieldType.GetMethod("RemoveEntry");
            s_ProjectBuildFilesGetForSourceFile = typeof(PBXProject).GetMethod("BuildFilesGetForSourceFile", pv);
            s_GroupsGetMainGroup = s_ProjectData.FieldType.GetMethod("GroupsGetMainGroup", pv | BindingFlags.Public);

            #if !UNITY_2023_1_OR_NEWER
            s_PBXBuildFileDat = asm.GetType($"{ns}.PBXBuildFileData");
            s_PBXElementArray = asm.GetType($"{ns}.PBXElementArray");
            s_PBXElementString = asm.GetType($"{ns}.PBXElementString");
            var fileGUIDListBase = asm.GetType($"{ns}.FileGUIDListBase");
            var pBXElementDict = asm.GetType($"{ns}.PBXElementDict");

            s_DataFileGroups = s_ProjectData.FieldType.GetField("groups", pv);
            s_ResourceFiles = fileGUIDListBase.GetField("files");
            s_PBXElementArrayValues = s_PBXElementArray.GetField("values");
            s_GroupName = group.GetField("name");
            s_GroupPath = group.GetField("path");

            s_GroupsObjects = s_DataFileGroups.FieldType.GetMethod("GetObjects");
            s_DataFileRefsFieldObjects = s_DataFileRefsField.FieldType.GetMethod("GetObjects");
            s_GetPropertiesRaw = pbxObject.GetMethod("GetPropertiesRaw", pv);
            s_FileRefDataCreateFromFile = fileRefData.GetMethod("CreateFromFile", BindingFlags.Static | BindingFlags.Public);
            s_PBXBuildFileDataCreateFromFile = s_PBXBuildFileDat.GetMethod("CreateFromFile", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(bool), typeof(string) }, null);
            s_ProjectBuildFilesAdd = typeof(PBXProject).GetMethod("BuildFilesAdd", pv);
            s_ProjectFileRefsAdd = typeof(PBXProject).GetMethod("FileRefsAdd", pv);

            s_FileRefsPath = fileRefData.GetProperty("path");
            s_KnownRegionsDict = pBXElementDict.GetField("m_PrivateValue", pv);
            s_ProjectSection = typeof(PBXProject).GetProperty("project", pv);
            s_ProjectSectionObjectData = s_ProjectSection.PropertyType.GetProperty("project");
            s_ProjectResoruces = typeof(PBXProject).GetProperty("resources", pv);

            s_RawPropertiesValuesGetValue = s_KnownRegionsDict.FieldType.GetMethod("TryGetValue");
            s_RawPropertiesValuesAddValue = s_KnownRegionsDict.FieldType.GetMethod("Add");
            s_ResorucesObjects = s_ProjectResoruces.PropertyType.GetMethod("GetObjects");
            s_VariantGroupsAddEntry = s_ProjectVariantGroups.PropertyType.GetMethod("AddEntry");
            s_VariantGroupsSetPropertyString = group.GetMethod("SetPropertyString", pv);
            #endif
        }

        /// <summary>
        /// Removes the locale entry from a <c>PBXVariantGroup</c>, used to strip
        /// pre-baked entries from the iOS trampoline pbxproj (LOC-1133). Public
        /// <see cref="PBXProject.RemoveFile"/> can't be used because variant-group
        /// children aren't tracked in the parent-group map and it would NRE.
        /// </summary>
        public static void RemoveLocaleFromVariantGroup(this PBXProject project, string variantGroupName, string localeCode)
        {
            try
            {
                var variantGroups = s_ProjectVariantGroups.GetValue(project);

                object variantGroup = null;
                foreach (var vg in s_VariantGroupsObjects.Invoke(variantGroups, null) as ICollection)
                {
                    if ((string)s_VariantGroupName.GetValue(vg) == variantGroupName)
                    {
                        variantGroup = vg;
                        break;
                    }
                }
                if (variantGroup == null)
                    return;

                var children = s_GroupChildren.GetValue(variantGroup);
                var data = s_ProjectData.GetValue(project);
                var fileRefs = s_DataFileRefsField.GetValue(data);

                string targetGuid = null;
                foreach (string childGuid in (IEnumerable)children)
                {
                    var fileRef = s_FileRefsIndexer.GetValue(fileRefs, new object[] { childGuid });
                    if (fileRef == null)
                        continue;
                    if ((string)s_FileRefName.GetValue(fileRef) == localeCode)
                    {
                        targetGuid = childGuid;
                        break;
                    }
                }
                if (targetGuid == null)
                    return;

                // Removing from children stops Xcode looking for the file; the resources
                // build phase references the variant group, not the child, so it's unaffected.
                s_GuidListRemove.Invoke(children, new object[] { targetGuid });
                s_FileRefsRemoveEntry.Invoke(fileRefs, new object[] { targetGuid });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to remove '{localeCode}' from {variantGroupName} variant group: {e}");
            }
        }

        /// <summary>
        /// Adds <paramref name="variantGroupName"/> to the project's mainGroup and to the
        /// main target's resources build phase. <see cref="PBXProject.AddLocaleVariantFile"/>
        /// only does this when an iOS-trampoline-only <c>CustomTemplate</c> group exists,
        /// so non-iOS Xcode builds otherwise report "0 Files localized" (UUM-132514).
        /// </summary>
        public static void IncludeVariantGroupInBuild(this PBXProject project, string variantGroupName)
        {
            try
            {
                var data = s_ProjectData.GetValue(project);
                var variantGroups = s_ProjectVariantGroups.GetValue(project);

                object variantGroup = null;
                foreach (var vg in s_VariantGroupsObjects.Invoke(variantGroups, null) as ICollection)
                {
                    if ((string)s_VariantGroupName.GetValue(vg) == variantGroupName)
                    {
                        variantGroup = vg;
                        break;
                    }
                }
                if (variantGroup == null)
                    return;

                var groupGuid = (string)s_PBXObjectGuid.GetValue(variantGroup);
                var mainTarget = project.GetUnityMainTargetGuid();

                var mainGroup = s_GroupsGetMainGroup.Invoke(data, null);
                var mainGroupChildren = s_GroupChildren.GetValue(mainGroup);
                if (!(bool)s_GUIDListContains.Invoke(mainGroupChildren, new object[] { groupGuid }))
                    s_GuidListAdd.Invoke(mainGroupChildren, new object[] { groupGuid });

                if (s_ProjectBuildFilesGetForSourceFile.Invoke(project, new object[] { mainTarget, groupGuid }) == null)
                {
                    var resourcesPhase = project.GetResourcesBuildPhaseByTarget(mainTarget);
                    project.AddFileToBuildSection(mainTarget, resourcesPhase, groupGuid);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to include {variantGroupName} variant group in the Xcode build: {e}");
            }
        }

        #if !UNITY_2023_1_OR_NEWER
        // Pre-2023.1 reimplementations of PBXProject APIs that became public in 2023.1.

        static /* PBXFileReferenceData */ object GetFileRefDataByPath(this PBXProject project, string path)
        {
            var data = s_ProjectData.GetValue(project);
            var fileRefs = s_DataFileRefsField.GetValue(data);
            var values = s_DataFileRefsFieldObjects.Invoke(fileRefs, null) as ICollection;

            // The lookup methods provided by PBXproject dont seem to be reliable so we will just go through the assets manually.
            foreach (var f in values)
            {
                var fileRefPath = s_FileRefsPath.GetValue(f) as string;
                if (fileRefPath == path)
                    return f;
            }
            return null;
        }

        static /* PBXGroupData */ object GetGroupByName(this PBXProject project, string name)
        {
            var data = s_ProjectData.GetValue(project);
            var groups = s_DataFileGroups.GetValue(data);
            var groupsValues = s_GroupsObjects.Invoke(groups, null) as ICollection;

            foreach (var g in groupsValues)
            {
                var groupName = s_GroupName.GetValue(g) as string;
                if (groupName == name)
                    return g;
            }
            return null;
        }

        static IList GetKnownRegions(this PBXProject project)
        {
            const string elementName = "knownRegions";

            var section = s_ProjectSection.GetValue(project);
            var data = s_ProjectSectionObjectData.GetValue(section);
            var rawProperties = s_GetPropertiesRaw.Invoke(data, null);

            object[] args = new[] { elementName, null };
            var dict = s_KnownRegionsDict.GetValue(rawProperties);
            var ret = (bool)s_RawPropertiesValuesGetValue.Invoke(dict, args);
            if (!ret)
            {
                args[1] = Activator.CreateInstance(s_PBXElementArray);
                s_RawPropertiesValuesAddValue.Invoke(dict, new object[] { elementName, args[1] });
            }

            return s_PBXElementArrayValues.GetValue(args[1]) as IList;
        }

        static string AddFileRefToBuild(this PBXProject project, string target, string guid)
        {
            var data = s_PBXBuildFileDataCreateFromFile.Invoke(null, new object[] { guid, false, null });
            s_ProjectBuildFilesAdd.Invoke(project, new object[] { target, data });
            return s_PBXObjectGuid.GetValue(data) as string;
        }

        static void AddFileToResourceBuildPhase(this PBXProject project, string buildPhaseGuid, string fileGuid)
        {
            var resources = s_ProjectResoruces.GetValue(project);

            var values = s_ResorucesObjects.Invoke(resources, null) as ICollection;
            foreach (var v in values)
            {
                var guid = s_PBXObjectGuid.GetValue(v) as string;
                if (guid == buildPhaseGuid)
                {
                    var files = s_ResourceFiles.GetValue(v);
                    s_GuidListAdd.Invoke(files, new object[] { fileGuid });
                }
            }
        }

        public static void SetDevelopmentRegion(this PBXProject project, string code)
        {
            const string elementName = "developmentRegion";

            var section = s_ProjectSection.GetValue(project);
            var data = s_ProjectSectionObjectData.GetValue(section);
            var rawProperties = s_GetPropertiesRaw.Invoke(data, null);
            var dict = s_KnownRegionsDict.GetValue(rawProperties) as IDictionary;
            dict[elementName] = Activator.CreateInstance(s_PBXElementString, code);
        }

        public static void ClearKnownRegions(this PBXProject project)
        {
            var regions = project.GetKnownRegions();
            regions.Clear();
        }

        public static void AddKnownRegion(this PBXProject project, string code)
        {
            var regions = project.GetKnownRegions();
            var element = Activator.CreateInstance(s_PBXElementString, code);
            regions.Add(element);
        }

        public static void AddLocaleVariantFile(this PBXProject project, string groupName, string code, string path)
        {
            /// Replaces '\' with '/'. We need to apply this function to all paths that come from the user
            /// of the API because we store paths to pbxproj and on windows we may get path with '\' slashes
            /// instead of '/' slashes
            path = path.Replace('\\', '/');

            // Get or create the variant group
            var variantGroups = s_ProjectVariantGroups.GetValue(project);
            var variantGroupValues = s_VariantGroupsObjects.Invoke(variantGroups, null) as ICollection;
            object group = null;
            foreach (var g in variantGroupValues)
            {
                var name = s_VariantGroupName.GetValue(g) as string;
                if (name == groupName)
                    group = g;
            }

            if (group == null)
            {
                var guid = Guid.NewGuid().ToString("N").Substring(8).ToUpper();

                group = Activator.CreateInstance(s_PBXVariantGroupData);
                s_VariantGroupName.SetValue(group, groupName);
                s_GroupPath.SetValue(group, groupName);
                s_PBXObjectGuid.SetValue(group, guid);
                s_GroupChildren.SetValue(group, Activator.CreateInstance(s_GUIDList));
                s_VariantGroupsSetPropertyString.Invoke(group, new object[] { "isa", "PBXVariantGroup" });

                s_VariantGroupsAddEntry.Invoke(variantGroups, new object[] { group });
            }

            var targetGuid = project.GetUnityMainTargetGuid();
            var groupGuid = s_PBXObjectGuid.GetValue(group) as string;

            var buildFileData = s_ProjectBuildFilesGetForSourceFile.Invoke(project, new object[] { targetGuid, groupGuid });
            if (buildFileData == null)
            {
                var customData = project.GetGroupByName("CustomTemplate");
                var children = s_GroupChildren.GetValue(customData);
                s_GuidListAdd.Invoke(children, new object[] { groupGuid });

                var buildFileGuid = project.AddFileRefToBuild(project.GetUnityMainTargetGuid(), groupGuid);
                var buildPhaseGuid = project.GetResourcesBuildPhaseByTarget(targetGuid);
                project.AddFileToResourceBuildPhase(buildPhaseGuid, buildFileGuid);
            }

            // Add the file if it has not already been added
            var fileRef = project.GetFileRefDataByPath(path);
            if (fileRef == null)
            {
                fileRef = s_FileRefDataCreateFromFile.Invoke(null, new object[] { path, code, PBXSourceTree.Source });
                s_ProjectFileRefsAdd.Invoke(project, new object[] { path, code, group, fileRef });
            }

            // Add the file to the variant group
            var fileRefsGuid = s_PBXObjectGuid.GetValue(fileRef) as string;
            var groupChildren = s_GroupChildren.GetValue(group);
            var res = (bool)s_GUIDListContains.Invoke(groupChildren, new object[] { fileRefsGuid });
            if (!res)
            {
                s_GuidListAdd.Invoke(groupChildren, new[] { fileRefsGuid });
            }
        }
        #endif
    }
}
#endif
