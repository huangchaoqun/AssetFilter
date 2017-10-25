/*************************************************************************
 *  Copyright (C), 2017-2018, Mogoson Tech. Co., Ltd.
 *------------------------------------------------------------------------
 *  File         :  AssetFilterEditor.cs
 *  Description  :  Editor to check the name of assets under the target
 *                  directory, filter and display the assets those name
 *                  is mismatch the define specification.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  8/18/2017
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Developer.AssetFilter
{
    public class AssetFilterEditor : EditorWindow
    {
        #region Property and Field
        private static AssetFilterEditor instance;
        private Thread thread;

        private const float labelWidth = 120;
        private const float buttonWidth = 80;
        private Vector2 scrollPos = Vector2.zero;

        private const string targetDirectoryKey = "AssetFilterTargetDirectory";
        private string targetDirectory = "Assets";

        private const string settingsPath = "Assets/AssetFilter/Settings/AssetPatternSettings.asset";
        private AssetPatternSettings patternSettings;

        private List<string> filterAssets = new List<string>();
        private int totalCount = 0, doneCount = 0;
        private int pageCount = 0, pageIndex = 0;
        private const int eachPageCount = 100;
        #endregion

        #region Private Method
        [MenuItem("Tool/Asset Filter &F")]
        private static void ShowEditor()
        {
            instance = GetWindow<AssetFilterEditor>("Asset Filter");
            instance.Show();
        }

        private void OnEnable()
        {
            targetDirectory = EditorPrefs.GetString(targetDirectoryKey, targetDirectory);
            patternSettings = AssetDatabase.LoadAssetAtPath(settingsPath, typeof(AssetPatternSettings)) as AssetPatternSettings;
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
            GUILayout.Label("Target Directory", GUILayout.Width(labelWidth));
            targetDirectory = GUILayout.TextField(targetDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(buttonWidth)))
                SelectDirectory();
            GUILayout.EndHorizontal();
            #endregion

            #region Pattern Settings
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pattern Settings", GUILayout.Width(labelWidth));
            EditorGUI.BeginChangeCheck();
            patternSettings = EditorGUILayout.ObjectField(patternSettings, typeof(AssetPatternSettings), false) as AssetPatternSettings;
            if (EditorGUI.EndChangeCheck())
                ClearEditorCache();
            if (GUILayout.Button("New", GUILayout.Width(buttonWidth)))
                CreateNewSettings();
            GUILayout.EndHorizontal();
            #endregion

            #region Top Tool Bar
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Check", GUILayout.Width(buttonWidth)))
                CheckAssetsName();
            if (GUILayout.Button("Clear", GUILayout.Width(buttonWidth)))
                ClearEditorCache();
            GUILayout.EndHorizontal();
            #endregion

            #region Mismatch Assets
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mismatch Assets", GUILayout.Width(labelWidth));
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
            if (pageCount > 1)
            {
                GUILayout.BeginHorizontal();
                if (pageIndex > 0)
                {
                    if (GUILayout.Button("Previous", GUILayout.Width(buttonWidth)))
                    {
                        pageIndex--;
                        scrollPos = Vector2.zero;
                    }
                }
                else
                    GUILayout.Label(string.Empty, GUILayout.Width(buttonWidth));

                GUILayout.FlexibleSpace();
                GUILayout.Label(pageIndex + 1 + " / " + pageCount);
                GUILayout.FlexibleSpace();

                if (pageIndex < pageCount - 1)
                {
                    if (GUILayout.Button("Next", GUILayout.Width(buttonWidth)))
                    {
                        pageIndex++;
                        scrollPos = Vector2.zero;
                    }
                }
                else
                    GUILayout.Label(string.Empty, GUILayout.Width(buttonWidth));
                GUILayout.EndHorizontal();
            }
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
                EditorPrefs.SetString(targetDirectoryKey, targetDirectory);
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
                        Thread.Sleep(0);
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

        private bool CheckMismatchPattern(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            if (extension == ".meta")
                return false;

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            foreach (var pattern in patternSettings.assetPatterns)
            {
                if (Regex.IsMatch(extension, pattern.extensionPattern) && !Regex.IsMatch(fileName, pattern.namePattern))
                    return true;
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