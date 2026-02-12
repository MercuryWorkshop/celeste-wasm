using System.Runtime.InteropServices;
using System;

namespace FMOD
{
    public struct DSP_DESCRIPTION { }
    public struct DSP_TYPE { }
    public struct DSP_PARAMETER_DESC { }
    public struct DSP_METERING_INFO { }
}

namespace FMOD.Studio {
    public class Other
    {
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_System_SetListenerAttributes_2(IntPtr system, int listener, ref ATTRIBUTES_3D attributes);
        [DllImport(STUDIO_VERSION.dll)]
        private static extern RESULT FMOD_Studio_EventInstance_SetParameterByName_2(IntPtr _event, byte[] name, float value);
    }
}
