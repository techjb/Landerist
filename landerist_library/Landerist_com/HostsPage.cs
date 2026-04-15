using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Websites;
using System;
using System.Collections.Generic;
using System.Text;

namespace landerist_library.Landerist_com
{
    public class HostsPage
    {
        private static readonly string HostsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "hosts_template.html");

        private static readonly string HostsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "hosts.html");

        private static string HostsTemplate = string.Empty;
        
        
        public static void Update()
        {
            try
            {
                HostsTemplate = File.ReadAllText(HostsTemplateHtmlFile);

                
            }
            catch (Exception exception)
            {
                Log.WriteError("DownloadsPage Update", exception);
            }
        }
    }
}
