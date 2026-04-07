using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace ForgePLM.SolidWorks.Addin
{
    [ComVisible(true)]
    [Guid("D4B4B9F2-7E61-4D3D-9D2B-6A9B61E8C101")]
    public class SwAddin : ISwAddin
    {
        private SldWorks _swApp;
        private int _addinCookie;
        private TaskpaneView _taskPaneView;
        private ForgePlmTaskPaneControl _taskPaneControl;

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            _swApp = (SldWorks)ThisSW;
            _addinCookie = Cookie;

            _swApp.SetAddinCallbackInfo2(0, this, _addinCookie);

            CreateTaskPane();
            HookSolidWorksEvents();

            _taskPaneControl?.OnActiveDocumentChanged();

            _swApp.SendMsgToUser2(
                "ForgePLM add-in connected.",
                (int)swMessageBoxIcon_e.swMbInformation,
                (int)swMessageBoxBtn_e.swMbOk);

            return true;
        }

        public bool DisconnectFromSW()
        {
            try
            {
                UnhookSolidWorksEvents();

                if (_taskPaneView != null)
                {
                    _taskPaneView.DeleteView();
                    _taskPaneView = null;
                }
            }
            catch
            {
                // Ignore cleanup errors for now
            }

            _taskPaneControl = null;
            _swApp = null;

            return true;
        }

        private void CreateTaskPane()
        {
            string iconPath = "";

            _taskPaneView = _swApp.CreateTaskpaneView2(iconPath, "ForgePLM");

            _taskPaneView.AddControl(
                "ForgePLM.SolidWorks.Addin.ForgePlmTaskPaneControl",
                "");

            object controlObject = _taskPaneView.GetControl();
            _taskPaneControl = controlObject as ForgePlmTaskPaneControl;

            if (_taskPaneControl != null)
            {
                _taskPaneControl.Initialize(_swApp);
            }
            else
            {
                _swApp.SendMsgToUser2(
                    "ForgePLM: failed to get task pane control instance.",
                    (int)swMessageBoxIcon_e.swMbStop,
                    (int)swMessageBoxBtn_e.swMbOk);
            }
        }

        private void HookSolidWorksEvents()
        {
            if (_swApp == null)
                return;

            _swApp.ActiveDocChangeNotify += OnActiveDocChangeNotify;
        }

        private void UnhookSolidWorksEvents()
        {
            if (_swApp == null)
                return;

            try
            {
                _swApp.ActiveDocChangeNotify -= OnActiveDocChangeNotify;
            }
            catch
            {
                // Ignore unhook errors for now
            }
        }

        private int OnActiveDocChangeNotify()
        {
            try
            {
                _taskPaneControl?.OnActiveDocumentChanged();
            }
            catch
            {
                // Keep SolidWorks stable even if our UI update fails
            }

            return 0;
        }

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            string addinKey = $@"SOFTWARE\SolidWorks\Addins\{{{t.GUID}}}";
            using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(addinKey))
            {
                rk.SetValue(null, 1);
                rk.SetValue("Title", "ForgePLM");
                rk.SetValue("Description", "ForgePLM SolidWorks Add-in");
            }

            string startupKey = $@"Software\SolidWorks\AddInsStartup\{{{t.GUID}}}";
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(startupKey))
            {
                rk.SetValue(null, 1);
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            string addinKey = $@"SOFTWARE\SolidWorks\Addins\{{{t.GUID}}}";
            Registry.LocalMachine.DeleteSubKey(addinKey, false);

            string startupKey = $@"Software\SolidWorks\AddInsStartup\{{{t.GUID}}}";
            Registry.CurrentUser.DeleteSubKey(startupKey, false);
        }
    }
}