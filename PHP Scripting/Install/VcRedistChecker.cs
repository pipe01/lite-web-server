using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHP_Scripting.Install
{
    public static class VcRedistChecker
    {
        public struct VcRedistributable
        {
            public int Version;
            public string Name;
            public string RegKey;
            public string DownloadURL;
            public bool Installed;
            public string InstallArgs;

            public VcRedistributable(string Name, int Version, string RegKey,
                string DownloadURL, bool Installed, string InstallArgs)
            {
                this.Version = Version;
                this.DownloadURL = DownloadURL;
                this.Name = Name;
                this.RegKey = RegKey;
                this.Installed = Installed;
                this.InstallArgs = InstallArgs;
            }
        }

        private static List<VcRedistributable> VcRedists = new List<VcRedistributable>();

        public static Dictionary<VcRedistributable, bool> InstalledRedists = new Dictionary<VcRedistributable, bool>();

        static VcRedistChecker()
        {
            AddRedist("2005", "6", @"SOFTWARE\Classes\Installer\Products\c1c4f01781cc94c4c8fb1542c0981a2a", "/q:a /c:\"VCREDI~1.EXE / q:a / c:\"\"msiexec / i vcredist.msi / qn\"\"", "https://download.microsoft.com/download/8/B/4/8B42259F-5D70-43F4-AC2E-4B208FD8D66A/vcredist_x86.EXE");
            AddRedist("2008", "9", @"SOFTWARE\Classes\Installer\Products\6E815EB96CCE9A53884E7857C57002F0", "/q", "https://download.microsoft.com/download/5/D/8/5D8C65CB-C849-4025-8E95-C3966CAFD8AE/vcredist_x86.exe");
            AddRedist("2010", "10", @"SOFTWARE\Classes\Installer\Products\1D5E3C0FEDA1E123187686FED06E995A", "/q /norestart", "https://download.microsoft.com/download/1/6/5/165255E7-1014-4D0A-B094-B6A430A6BFFC/vcredist_x86.exe");
            AddRedist("2012", "11", @"SOFTWARE\Classes\Installer\Dependencies\{33d1fd90-4274-48a1-9bc1-97e33d9c2d6f}", "/q /norestart", "https://download.microsoft.com/download/1/6/B/16B06F60-3B20-4FF2-B699-5E9B7962F9AE/VSU_4/vcredist_x86.exe");
            AddRedist("2013", "12", @"SOFTWARE\Classes\Installer\Dependencies\{f65db027-aff3-4070-886a-0d87064aabb1}", "/install /passive /norestart", "https://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x86.exe");
            AddRedist("2015", "14", @"SOFTWARE\Classes\Installer\Dependencies\{e2803110-78b3-4664-a479-3611a381656a}", "/install /passive /norestart", "https://download.microsoft.com/download/6/A/A/6AA4EDFF-645B-48C5-81CC-ED5963AEAD48/vc_redist.x86.exe");

            foreach (var item in VcRedists)
            {
                InstalledRedists.Add(item, DoesKeyExist(item.RegKey));
            }
        }
        private static void AddRedist(string name, string version, string key, string args, string url)
        {
            VcRedists.Add(new VcRedistributable(name, int.Parse(version), key, url, DoesKeyExist(key), args));
        }

        private static bool DoesKeyExist(string key)
        {
            using (var reg = Registry.LocalMachine.OpenSubKey(key))
                return reg != null;
        }

        public static bool IsVCInstalled(VcRedistributable vc)
        {
            return VcRedists.Contains(vc) && DoesKeyExist(vc.RegKey);
        }

        public static VcRedistributable GetVC(int version)
        {
            return VcRedists.FirstOrDefault(o => o.Version == version);
        }
    }
}
