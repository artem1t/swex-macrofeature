﻿//**********************
//SwEx.MacroFeature - framework for developing macro features in SOLIDWORKS
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/swex-macrofeature/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/macro-feature
//**********************

using CodeStack.SwEx.MacroFeature.Helpers;
using CodeStack.SwEx.MacroFeature.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolidWorks.Interop.sldworks
{
    public static class DimensionEx
    {
        private static readonly IMathUtility m_MathUtils;

        static DimensionEx()
        {
            m_MathUtils = Context.CurrentApp.IGetMathUtility();
        }

        public static void SetDirection(this IDimension dim,
            Point originPt, Vector dir, double length, Vector extDir = null)
        {
            var dimDirVec = m_MathUtils.CreateVector(dir.ToArray()) as MathVector;
            var startPt = m_MathUtils.CreatePoint(originPt.ToArray()) as IMathPoint;
            var endPt = m_MathUtils.CreatePoint(originPt.Move(dir, length).ToArray()) as IMathPoint;

            var refPts = new IMathPoint[] 
            {
                startPt,
                endPt,
                m_MathUtils.CreatePoint(new double[3]) as IMathPoint
            };

            if (extDir == null)
            {
                var yVec = new Vector(0, 1, 0);
                if (dir.IsSame(yVec))
                {
                    extDir = new Vector(1, 0, 0);
                }
                else
                {
                    extDir = yVec.Cross(dir);
                }
            }

            var extDirVec = m_MathUtils.CreateVector(extDir.ToArray()) as MathVector;
            
            dim.DimensionLineDirection = dimDirVec;
            dim.ExtensionLineDirection = extDirVec;
            dim.ReferencePoints = refPts;
        }
    }
}
