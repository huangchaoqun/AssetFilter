/*************************************************************************
 *  Copyright (C), 2017-2018, Mogoson tech. Co., Ltd.
 *  FileName: AssetFilterEditor.cs
 *  Author: Mogoson   Version: 1.0   Date: 8/18/2017
 *  Version Description:
 *    Internal develop version,mainly to achieve its function.
 *  File Description:
 *    Ignore.
 *  Class List:
 *    <ID>           <name>             <description>
 *     1.       AssetFilterEditor          Ignore.
 *  Function List:
 *    <class ID>     <name>             <description>
 *     1.
 *  History:
 *    <ID>    <author>      <time>      <version>      <description>
 *     1.     Mogoson     8/18/2017        1.0         Build this file.
 *************************************************************************/

namespace Developer.AssetFilter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;

    public class AssetFilterEditor : EditorWindow
    {
        #region Property and Field
        private static AssetFilterEditor instance;
        private Thread thread;

        private const float leftAlignment = 120;
        private const float rightAlignment = 80;
        private Vector2 scrollPos = Vector2.zero;

        private const string lastDirectory = "LastDirectory";
        private string targetDirectory = "Assets";

        private const string settingsPath = "Assets/AssetFilter/Settings/AssetPatternSettings.asset";
        private AssetPatternSettings patternSettings;

        private List<string> filterAssets = new List<string>();
        private int totalCount = 0, doneCount = 0;
        private int pageCount = 0, pageIndex = 0;
        private const int eachPageCount = 100;
        #endregion

        #region Private Method
        [MenuItem("Tool/Asset Filter &A")]
        private static void ShowEditor()
        {
            instance = GetWindow<AssetFilterEditor>("Asset Filter");
            instance.Show();
        }

        private void OnEnable()
        {
            targetDirectory = EditorPrefs.GetString(lastDirectory, targetDirectory);
            patternSettings = AssetDatabase.LoadAssetAtPath<AssetPatternSettings>(settingsPath);
        }

        private void OnDisable()
        {
            EditorUtility.UnloadUnusedAssetsImmediate(true);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(string.Empty, "Window");

            #region Select Directory
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Directory", GUILayout.Width(leftAlignment));
            targetDirectory = GUILayout.TextField(targetDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(rightAlignment)))
                SelectDirectory();
            GUILayout.EndHorizontal();
            #endregion

            #region Pattern Settings
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pattern Settings", GUILayout.Width(leftAlignment));
            EditorGUI.BeginChangeCheck();
            patternSettings = EditorGUILayout.ObjectField(patternSettings, typeof(AssetPatternSettings), false) as AssetPatternSettings;
            if (EditorGUI.EndChangeCheck())
                ClearEditorCache();
            if (GUILayout.Button("New", GUILayout.Width(rightAlignment)))
                CreateNewSettings();
            GUILayout.EndHorizontal();
            #endregion

            #region Top Tool Bar
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Check", GUILayout.Width(rightAlignment)))
                CheckAssetsName();
            if (GUILayout.Button("Clear", GUILayout.Width(rightAlignment)))
                ClearEditorCache();
            GUILayout.EndHorizontal();
            #endregion

            #region Mismatch Assets
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mismatch Assets", GUILayout.Width(leftAlignment));
            GUILayout.Label(filterAssets.Count.ToString());
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos, "Box");
            var startIndex = pageIndex * eachPageCount;
            var limitCount = Math.Min(eachPageCount, filterAssets.Count - startIndex);
            for (int i = startIndex; i < startIndex + limitCount; i++)
            {
                if (GUILayout.Button(filterAssets[i], "TextField"))
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath(filterAssets[i], typeof(UnityEngine.Object));
            }
            GUILayout.EndScrollView();
            #endregion

            #region Bottom Tool Bar
            GUILayout.BeginHorizontal();
            if (pageIndex > 0)
            {
                if (GUILayout.Button("Previous", GUILayout.Width(rightAlignment)))
                {
                    pageIndex--;
                    scrollPos = Vector2.zero;
                }
            }
            GUILayout.FlexibleSpace();
            if (pageIndex < pageCount - 1)
            {
                if (GUILayout.Button("Next", GUILayout.Width(rightAlignment)))
                {
                    pageIndex++;
                    scrollPos = Vector2.zero;
                }
            }
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndVertical();
        }

        private void Update()
        {
            if (doneCount < totalCount)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Check Assets Name",
                    doneCount + " / " + totalCount + " of assets have been checked.",
                    (float)doneCount / totalCount))
                {
                    thread.Abort();
                    doneCount = totalCount;
                    pageCount = GetCurrentPageCount();
                }
            }
            else if (totalCount > 0)
            {
                EditorUtility.ClearProgressBar();
                doneCount = totalCount = 0;
            }
        }

        private void SelectDirectory()
        {
            var selectDirectory = EditorUtility.OpenFolderPanel("Select Target Directory", targetDirectory, string.Empty);
            if (selectDirectory == string.Empty)
                return;

            ClearEditorCache();
            try
            {
                targetDirectory = selectDirectory.Substring(selectDirectory.IndexOf("Assets"));
                EditorPrefs.SetString(lastDirectory, targetDirectory);
            }
            catch
            {
                ShowNotification(new GUIContent("Invalid selection directory!"));
            }
        }

        private void CreateNewSettings()
        {
            ClearEditorCache();
            patternSettings = AssetPatternSettings.CreateDefaultInstance();
            AssetDatabase.CreateAsset(patternSettings, settingsPath);
            Selection.activeObject = patternSettings;
        }

        private void CheckAssetsName()
        {
            ClearEditorCache();
            if (!Directory.Exists(targetDirectory))
            {
                ShowNotification(new GUIContent("The target directory is not exist!"));
                return;
            }
            if (!patternSettings)
            {
                ShowNotification(new GUIContent("The pattern settings can not be null!"));
                return;
            }
            try
            {
                thread = new Thread(() =>
                {
                    var searchFiles = Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories);
                    totalCount = searchFiles.Length;
                    foreach (var file in searchFiles)
                    {
                        if (CheckMismatchPattern(file))
                            filterAssets.Add(file);
                        doneCount++;
                    }
                    pageCount = GetCurrentPageCount();
                });
                thread.Start();
            }
            catch (Exception e)
            {
                ShowNotification(new GUIContent(e.Message));
            }
        }

        private void ClearEditorCache()
        {
            filterAssets.Clear();
            totalCount = doneCount = 0;
            pageCount = pageIndex = 0;
        }

        private bool CheckMismatchPattern(string fileName)
        {
            foreach (var pattern in patternSettings.assetPatterns)
            {
                var extension = Path.GetExtension(fileName);
                if (extension != ".meta" &&
                    Regex.IsMatch(extension, pattern.extensionPattern) &&
                    !Regex.IsMatch(Path.GetFileNameWithoutExtension(fileName), pattern.namePattern))
                {
                    return true;
                }
            }
            return false;
        }

        private int GetCurrentPageCount()
        {
            return filterAssets.Count / eachPageCount + (filterAssets.Count % eachPageCount == 0 ? 0 : 1);
        }
        #endregion
    }
}