using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SwApp = SldWorks.SldWorks;
using SwModelDoc = SldWorks.ModelDoc2;

namespace ForgePLM.Administrator.Services
{
    public class SolidWorksArtifactExportService
    {
        

        private static SldWorks.SldWorks GetOrStartSolidWorks()
        {
            object? activeObject;

            if (TryGetActiveObject("SldWorks.Application", out activeObject) && activeObject != null)
                return (SwApp)activeObject;

            Type? swType = Type.GetTypeFromProgID("SldWorks.Application");

            if (swType == null)
                throw new InvalidOperationException("SolidWorks is not installed or COM registration is missing.");

            object? instance = Activator.CreateInstance(swType);

            if (instance == null)
                throw new InvalidOperationException("Failed to create SolidWorks instance.");

            return (SwApp)instance;
        }

        private static bool TryGetActiveObject(string progId, out object? activeObject)
        {
            activeObject = null;

            Guid clsid;
            int hr = CLSIDFromProgID(progId, out clsid);

            if (hr < 0)
                return false;

            try
            {
                GetActiveObject(ref clsid, IntPtr.Zero, out activeObject);
                return activeObject != null;
            }
            catch
            {
                activeObject = null;
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("ole32.dll")]
        private static extern int CLSIDFromProgID(
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string progId,
        out Guid clsid);

        [System.Runtime.InteropServices.DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
            ref Guid rclsid,
            IntPtr pvReserved,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object? ppunk);


    }
}