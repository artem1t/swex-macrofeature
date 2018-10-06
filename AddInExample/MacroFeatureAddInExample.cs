﻿using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.MacroFeature.Example.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using SolidWorksTools;
using SolidWorksTools.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeStack.SwEx.MacroFeature.Example
{
    public class MyParams
    {
    }

    [ComVisible(true)]
    [Icon(typeof(Resources), nameof(Resources.codestack))]
    public class MyMacroFeature : MacroFeatureEx<MyParams>
    {
    }

    [Guid("E5A3CBAF-75AE-4120-85BB-1A5784D738BC"), ComVisible(true)]
    [SwAddin(
    Description = "SwEx.MacroFeature example add-in",
    Title = "SwEx.MacroFeature.Example",
    LoadAtStartup = true
    )]
    public class MacroFeatureAddInExample : ISwAddin
    {
        private ISldWorks m_App;
        private ICommandManager m_CmdMgr;
        private int m_AddinID;
        private BitmapHandler m_Bmp;

        public const int CMD_GRP_ID = 0;
        public const int CMD_PMP_ID = 1;

        #region SolidWorks Registration

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            try
            {
                var att = t.GetCustomAttributes(false).OfType<SwAddinAttribute>().FirstOrDefault();

                if (att == null)
                {
                    throw new NullReferenceException($"{typeof(SwAddinAttribute).FullName} is not set on {t.GetType().FullName}");
                }

                Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                Microsoft.Win32.RegistryKey addinkey = hklm.CreateSubKey(keyname);
                addinkey.SetValue(null, 0);

                addinkey.SetValue("Description", att.Description);
                addinkey.SetValue("Title", att.Title);

                keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                addinkey = hkcu.CreateSubKey(keyname);
                addinkey.SetValue(null, Convert.ToInt32(att.LoadAtStartup), Microsoft.Win32.RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while registering the addin: " + ex.Message);
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            try
            {
                Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                hklm.DeleteSubKey(keyname);

                keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                hkcu.DeleteSubKey(keyname);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while unregistering the addin: " + e.Message);
            }
        }

        #endregion

        public MacroFeatureAddInExample()
        {
        }

        public bool ConnectToSW(object ThisSW, int cookie)
        {
            m_App = (ISldWorks)ThisSW;
            m_AddinID = cookie;
            m_Bmp = new BitmapHandler();

            m_App.SetAddinCallbackInfo(0, this, m_AddinID);
            
            m_CmdMgr = m_App.GetCommandManager(cookie);
            AddCommandMgr();

            return true;
        }

        public void CreateMacroFeature()
        {
            m_App.IActiveDoc2.FeatureManager.InsertComFeature<MyMacroFeature, MyParams>(new MyParams());
        }

        public bool DisconnectFromSW()
        {
            RemoveCommandMgr();

            Marshal.ReleaseComObject(m_CmdMgr);
            m_CmdMgr = null;
            Marshal.ReleaseComObject(m_App);
            m_App = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return true;
        }

        private void AddCommandMgr()
        {
            ICommandGroup cmdGroup;

            int cmdIndex;

            var docTypes = new int[]
            {
                (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swDocumentTypes_e.swDocDRAWING,
                (int)swDocumentTypes_e.swDocPART
            };

            var thisAssembly = Assembly.GetAssembly(this.GetType());

            int cmdGroupErr = 0;
            bool ignorePrevious = false;

            object registryIDs;

            bool getDataResult = m_CmdMgr.GetGroupDataFromRegistry(CMD_GRP_ID, out registryIDs);

            var knownIDs = new int[] { CMD_PMP_ID };

            if (getDataResult)
            {
                if (!CompareIDs((int[])registryIDs, knownIDs))
                {
                    ignorePrevious = true;
                }
            }

            cmdGroup = m_CmdMgr.CreateCommandGroup2(CMD_GRP_ID, "SwEx.MacroFeature", "SwEx.MacroFeature example", "", -1, ignorePrevious, ref cmdGroupErr);
            cmdGroup.LargeIconList = m_Bmp.CreateFileFromResourceBitmap("AddIn.Icons.IconLarge.bmp", thisAssembly);
            cmdGroup.SmallIconList = m_Bmp.CreateFileFromResourceBitmap("AddIn.Icons.IconSmall.bmp", thisAssembly);
            cmdGroup.LargeMainIcon = m_Bmp.CreateFileFromResourceBitmap("AddIn.Icons.IconLarge.bmp", thisAssembly);
            cmdGroup.SmallMainIcon = m_Bmp.CreateFileFromResourceBitmap("AddIn.Icons.IconSmall.bmp", thisAssembly);

            int menuToolbarOption = (int)(swCommandItemType_e.swMenuItem | swCommandItemType_e.swToolbarItem);
            cmdIndex = cmdGroup.AddCommandItem2("CreateMacroFeature", -1, "Creates sample macro feature",
                "CreateMacroFeature", 0, "CreateMacroFeature", "", CMD_PMP_ID, menuToolbarOption);

            cmdGroup.HasToolbar = true;
            cmdGroup.HasMenu = true;
            cmdGroup.Activate();
        }

        private void RemoveCommandMgr()
        {
            m_Bmp.Dispose();

            m_CmdMgr.RemoveCommandGroup(CMD_GRP_ID);
        }

        private bool CompareIDs(int[] storedIDs, int[] addinIDs)
        {
            var storedList = new List<int>(storedIDs);
            var addinList = new List<int>(addinIDs);

            addinList.Sort();
            storedList.Sort();

            return addinList.SequenceEqual(storedList);
        }
    }
}