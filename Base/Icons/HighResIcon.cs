﻿//**********************
//SwEx - development tools for SOLIDWORKS
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/swex-macrofeature/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex-macrofeature
//**********************

using CodeStack.SwEx.Common.Icons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.MacroFeature.Icons
{
    internal class HighResIcon : IMacroFeatureIcon
    {
        internal HighResIcon(string baseName)
        {
            BaseName = baseName;
        }

        public Image Small { get; set; }
        public Image Medium { get; set; }
        public Image Large { get; set; }

        public string BaseName
        {
            get;
            private set;
        }
        
        public IEnumerable<IconSizeInfo> GetHighResolutionIconSizes()
        {
            yield return new IconSizeInfo(Small, MacroFeatureIconInfo.SizeHighResSmall, BaseName);
            yield return new IconSizeInfo(Medium, MacroFeatureIconInfo.SizeHighResMedium, BaseName);
            yield return new IconSizeInfo(Large, MacroFeatureIconInfo.SizeHighResLarge, BaseName);
        }

        public IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Small, MacroFeatureIconInfo.Size, BaseName);
        }
    }
}