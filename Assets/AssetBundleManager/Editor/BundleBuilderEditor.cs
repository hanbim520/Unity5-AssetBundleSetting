﻿///////////////////////////////////////////////////////////////////////////////
///
/// BundleBuilder.cs
/// 
/// (c)2016 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AssetBundles
{
    /// <summary>
    /// Custom inspector editor class to provide assetbundle build options.
    /// </summary>
    [CustomEditor(typeof(BundleBuilder))]
    public class BundleBuilderEditor : Editor
    {
        BundleBuilder builder;

        void OnEnable()
        {
            builder = target as BundleBuilder;
        }

        public override void OnInspectorGUI()
        {
            // Add header
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = Color.white;
            headerStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("AssetBundle Setting Tool", headerStyle, GUILayout.Height(20));
            EditorGUILayout.Space();

            // AssetBundle Options
            EditorGUILayout.LabelField("AssetBundle Options:", EditorStyles.boldLabel);
            string[] names = Enum.GetNames(typeof(BuildAssetBundleOptions));
            for(int i=0; i<names.Length; i++)
            {
                if (string.IsNullOrEmpty(names[i]) || names[i] == "None")
                    continue;

                BuildAssetBundleOptions key = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), names[i]);
                if (builder.EnabledOptions.ContainsKey(key))
                {
                    // provides toggle with tooltip.
                    GUIContent toggleContent = new GUIContent(" " + names[i], GetTooltip(key));
                    builder.EnabledOptions[key] = EditorGUILayout.ToggleLeft(toggleContent, builder.EnabledOptions[key]);
                }
            }

#if !(UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2) // for the version of Unity over 5.3x
            bool uncompressedAssetBundle;
            if (builder.EnabledOptions.TryGetValue(BuildAssetBundleOptions.UncompressedAssetBundle, out uncompressedAssetBundle))
            {
                if (uncompressedAssetBundle)
                {
                    bool chunkBasedCompression;
                    if (builder.EnabledOptions.TryGetValue(BuildAssetBundleOptions.ChunkBasedCompression, out chunkBasedCompression))
                    {
                        if (chunkBasedCompression)
                        {
                            if (EditorUtility.DisplayDialog("NOTE",
                                "Force disable 'UncompressedAssetBundle' option due to the 'ChunkBasedCompression' option is enabled.", "Ok"))
                            {
                                builder.EnabledOptions[BuildAssetBundleOptions.UncompressedAssetBundle] = false;
                            }
                        }
                    }
                }
            }
#endif

#if UNITY_5
            // DisableWriteTypeTree conflicts with IngoreTypeTreeChanges. So you can’t enable both of the options.
            bool disableWriteTypeTree;
            if (builder.EnabledOptions.TryGetValue(BuildAssetBundleOptions.DisableWriteTypeTree, out disableWriteTypeTree))
            {
                if (disableWriteTypeTree)
                {
                    bool ignoreTypeTreeChanges;
                    if (builder.EnabledOptions.TryGetValue(BuildAssetBundleOptions.IgnoreTypeTreeChanges, out ignoreTypeTreeChanges))
                    {
                        if (ignoreTypeTreeChanges)
                        {
                            //builder.EnabledOptions[BuildAssetBundleOptions.IgnoreTypeTreeChanges] = false;
                            if (EditorUtility.DisplayDialog("NOTE",
                                "You can’t ignore type tree changes if you disable type tree.", "Ok"))
                            {
                                builder.EnabledOptions[BuildAssetBundleOptions.IgnoreTypeTreeChanges] = false;
                            }
                        }
                    }
                }
            }
#endif
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Use Default Setting", GUILayout.Width(150)))
                {
                    UseDefaultSetting();
                }
            }
            EditorGUILayout.Space();

            // Output path setting
            EditorGUILayout.LabelField("Output Path:", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                builder.outputPath = GUILayout.TextField(builder.outputPath, GUILayout.MinWidth(250));
                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    // unity editor expects the current folder to be set to the project folder at all times.
                    string projectFolder = System.IO.Directory.GetCurrentDirectory();
                    string path = string.Empty;
                    path = EditorUtility.OpenFolderPanel("Select folder", projectFolder, "");
                    if (path.Length != 0)
                    {
                        builder.outputPath = path;
                    }
                }
            }
            EditorGUILayout.Space();

            // Build
            EditorGUILayout.LabelField("AssetBundle Build:", EditorStyles.boldLabel);
            if (GUILayout.Button("Build"))
            {
                //HACK: To prevent InvalidOperationException.
                //  See http://answers.unity3d.com/questions/852155/invalidoperationexception-operation-is-not-valid-d-1.html
                EditorApplication.delayCall += builder.Build;
            }
        }

        /// <summary>
        /// Returns correspond tooltip string with BuildAssetBundleOptions.
        /// </summary>
        private string GetTooltip(BuildAssetBundleOptions option)
        {
            if (option == BuildAssetBundleOptions.UncompressedAssetBundle)
                return "Don't compress the data when creating the asset bundle.";
            if (option == BuildAssetBundleOptions.DisableWriteTypeTree)
                return "Do not include type information within the AssetBundle.";
            if (option == BuildAssetBundleOptions.DeterministicAssetBundle)
                return "Builds an asset bundle using a hash for the id of the object stored in the asset bundle.";
            if (option == BuildAssetBundleOptions.ForceRebuildAssetBundle)
                return "Force rebuild the assetBundles.";
            if (option == BuildAssetBundleOptions.IgnoreTypeTreeChanges)
                return "Ignore the type tree changes when doing the incremental build check.";
            if (option == BuildAssetBundleOptions.AppendHashToAssetBundleName)
                return "Append the hash to the assetBundle name.";
#if !(UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2) // for the version of Unity over 5.3x
            if (option == BuildAssetBundleOptions.ChunkBasedCompression)
                return "Use chunk-based LZ4 compression when creating the AssetBundle.";
#endif

            return string.Empty;
        }

        /// <summary>
        /// Disable all options. Same as BuildAssetBundleOptions.None
        /// </summary>
        private void UseDefaultSetting()
        {
            foreach(BuildAssetBundleOptions key in builder.EnabledOptions.Keys)
            {
                builder.EnabledOptions[key] = false;
            }
        }
    }
}