/*************************************************************************
 *  Copyright (C), 2017-2018, Mogoson Tech. Co., Ltd.
 *  FileName: AssetPatternSettings.cs
 *  Author: Mogoson   Version: 0.1.0   Date: 8/18/2017
 *  Version Description:
 *    Internal develop version,mainly to achieve its function.
 *  File Description:
 *    Ignore.
 *  Class List:
 *    <ID>           <name>             <description>
 *     1.     AssetPatternSettings         Ignore.
 *  Function List:
 *    <class ID>     <name>             <description>
 *     1.
 *  History:
 *    <ID>    <author>      <time>      <version>      <description>
 *     1.     Mogoson     8/18/2017       0.1.0       Create this file.
 *************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Developer.AssetFilter
{
    [Serializable]
    public struct AssetPattern
    {
        public string assetType;
        public string namePattern;
        public string extensionPattern;

        public AssetPattern(string assetType, string namePattern, string extensionPattern)
        {
            this.assetType = assetType;
            this.namePattern = namePattern;
            this.extensionPattern = extensionPattern;
        }
    }

    public class AssetPatternSettings : ScriptableObject
    {
        #region Property and Field
        public List<AssetPattern> assetPatterns = new List<AssetPattern>();
        #endregion

        #region Public Method
        public static AssetPatternSettings CreateDefaultInstance()
        {
            var instance = CreateInstance<AssetPatternSettings>();
            instance.assetPatterns.Add(new AssetPattern("Script", "^[A-Z]+[A-Za-z]+$", ".cs|.js"));
            instance.assetPatterns.Add(new AssetPattern("Model", "^[A-Z]+[A-Za-z0-9]+$", ".fbx|.obj|.max|.3ds|.blend|.dae|.dxf"));
            instance.assetPatterns.Add(new AssetPattern("Material", "^[A-Z]+(_?[A-Za-z0-9]+)+$", ".mat"));
            instance.assetPatterns.Add(new AssetPattern("Texture", "^[A-Z]+(_?[A-Za-z0-9]+)+$", ".jpg|.png|.tga|.bmp|.psd|.gif|.iff|.tiff|.pict"));
            return instance;
        }
        #endregion
    }
}