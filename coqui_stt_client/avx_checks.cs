using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace coqui_stt_client
{
    internal class avx_checks
    {
        public static Tuple<string, bool> isAvxSupported()
        {
            ManagementObjectSearcher mso = new ManagementObjectSearcher("select * from Win32_Processor");
            String cpu_name = String.Empty;

            foreach (ManagementObject mo in mso.Get())
            {
                cpu_name = mo["Name"].ToString();
                break;
            }

            bool hasAvx = false;

            if (cpu_name.Contains("Celeron"))
                hasAvx = false;
            else if ((cpu_name.Contains("Core(TM)")
                &&
                (cpu_name.Contains("i3") ||
                        cpu_name.Contains("i5") ||
                        cpu_name.Contains("i7") ||
                        cpu_name.Contains("i9"))
                )
            )
            {
                hasAvx = true;
            }
            else
            {
                hasAvx = HasAvxSupport();
            }

            return new Tuple<string, bool>(cpu_name, hasAvx);

        }


        private static bool HasAvxSupport()
        {
            try
            {
                long avxStatus = GetEnabledXStateFeatures();
                return (avxStatus & 4) != 0;
            }
            catch
            {
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern long GetEnabledXStateFeatures();

    }
}
