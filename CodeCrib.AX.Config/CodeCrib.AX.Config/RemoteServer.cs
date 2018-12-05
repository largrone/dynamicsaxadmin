using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Security;

namespace CodeCrib.AX.Config
{
    [Serializable]
    public class RemoteServer : Server
    {
        private RegistryKey _regKey;

        [XmlIgnore]
        public string ServerName
        {
            get;
            set;
        }

        protected RegistryKey RegistryKeyForServer
        {
            get
            {
                RegistryKey ret = null;

                if (_regKey != null)
                    ret = _regKey;
                else
                {
                    if (ServerName != "")
                    {
                        ret = GetRegistryKeyForServer(ServerName);
                    }

                    _regKey = ret;
                }

                return ret;
            }
        }

        private static RegistryKey GetRegistryKeyForServer(String serverName)
        {
            RegistryKey ret;

            if (serverName != System.Environment.MachineName)
            {
                try
                {
                    ret = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, serverName);
                    ret = ret.OpenSubKey(aosRegistryPath);
                }
                catch (SecurityException se)
                {
                    throw new Exception(String.Format("Could not open remote registry on server {0}", serverName), se);
                }
            }
            else
            {
                ret = Registry.LocalMachine.OpenSubKey(aosRegistryPath);
            }

            return ret;
        }

        public static List<string> GetAOSInstancesForServer(String serverName)
        {
            RegistryKey configKey = configKey = GetRegistryKeyForServer(serverName);
            List<string> instances = new List<string>();

            foreach (string aosNumberName in configKey.GetSubKeyNames())
            {
                RegistryKey aosInstance = configKey.OpenSubKey(aosNumberName);
                String instanceName = aosInstance.GetValue("InstanceName", null) as String;

                if (!String.IsNullOrEmpty(instanceName))
                {
                    instances.Add(instanceName);
                }
            }

            return instances;
        }

        public static RemoteServer GetConfigFromRegistryRemote(String serverName, uint port)
        {
            RegistryKey configKey = GetRegistryKeyForServer(serverName);
            uint aosNumber = 0;
            RemoteServer serverConfig = null;

            try
            {
                foreach (string aosNumberName in configKey.GetSubKeyNames())
                {
                    RegistryKey currentConfigKey = null;
                    String currentConfigurationName = "";

                    RegistryKey aosInstance;
                    try
                    {
                        aosInstance = configKey.OpenSubKey(aosNumberName);
                        currentConfigurationName = aosInstance.GetValue("Current", null) as String;
                    }
                    catch (SecurityException se)
                    {
                        throw new Exception(String.Format("Could not open registry setting for AOS {2}  on server {0} (scanning for port {1})  aosRegistryPath = {3}, path = {4}",
                            serverName, port, aosNumberName, aosRegistryPath, configKey.ToString()), se);
                    }

                    if (!String.IsNullOrEmpty(currentConfigurationName))
                    {
                        String currentPort;
                        try
                        {
                            currentConfigKey = aosInstance.OpenSubKey(currentConfigurationName);
                            currentPort = currentConfigKey.GetValue("port", null) as String;
                        }
                        catch (SecurityException se)
                        {
                            throw new Exception(String.Format("Could not open registry setting for configuration {3} on AOS {2}  on server {0} (scanning for port {1})", serverName, port, aosNumberName, currentConfigurationName), se);
                        }

                        if (UInt16.Parse(currentPort) == port)
                        {
                            aosNumber = UInt16.Parse(aosNumberName);
                            string versionOrigin = (string)aosInstance.GetValue("ProductVersion");

                            serverConfig = new RemoteServer() { ServerName = serverName, FormatVersion = "1", Configuration = currentConfigurationName, AOSNumberOrigin = aosNumber, AOSVersionOrigin = versionOrigin };

                            ConfigBase.GetConfigFromRegistry(serverConfig, currentConfigKey);
                            break;
                        }
                    }
                }
            }
            catch (SecurityException se)
            {
                throw new Exception(String.Format("Could not traverse AOS instances in server {0} to find port {1} due to security", serverName, port), se);
            }

            if (serverConfig == null)
            {
                throw new NullReferenceException(String.Format("Did not find Server Configuration for server {0} port {1}", serverName, port));
            }

            return serverConfig;
        }


        public static List<RemoteServer> GetServerConfigsFromClientConfig(Client clientConfig)
        {
            List<RemoteServer> servers = new List<RemoteServer>();

            var connections = from c in clientConfig.Connections orderby c.ServerName select c;

            foreach (var connection in connections)
            {
                servers.Add(GetConfigFromRegistryRemote(connection.ServerName, connection.TCPIPPort));
            }

            return servers;
        }


    }
}
