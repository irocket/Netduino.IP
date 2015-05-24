////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Net.NetworkInformation
{
    public enum NetworkInterfaceType
    {
        Unknown = 1,
        Ethernet = 6,
        Wireless80211 = 71,
    }

    public class NetworkInterface
    {
        //set update flags...
        private const int UPDATE_FLAGS_DNS = 0x1;
        private const int UPDATE_FLAGS_DHCP = 0x2;
        private const int UPDATE_FLAGS_DHCP_RENEW = 0x4;
        private const int UPDATE_FLAGS_DHCP_RELEASE = 0x8;
        private const int UPDATE_FLAGS_MAC = 0x10;

        private const uint FLAGS_DHCP = 0x1;
        private const uint FLAGS_DYNAMIC_DNS = 0x2;

        [FieldNoReflection]
        private readonly int _interfaceIndex;

        private uint _flags;
        private uint _ipAddress;
        private uint _gatewayAddress;
        private uint _subnetMask;
        private uint _dnsAddress1;
        private uint _dnsAddress2;
        private NetworkInterfaceType _networkInterfaceType;
        private byte[] _macAddress;

        protected NetworkInterface(int interfaceIndex)
        {
            this._interfaceIndex = interfaceIndex;
            _networkInterfaceType = NetworkInterfaceType.Unknown;
        }

        public static NetworkInterface[] GetAllNetworkInterfaces()
        {
            int count = GetNetworkInterfaceCount();
            NetworkInterface[] ifaces = new NetworkInterface[count];

            for (uint i = 0; i < count; i++)
            {
                ifaces[i] = GetNetworkInterface(i);
            }

            return ifaces;
        }

        private static int GetNetworkInterfaceCount()
        {
            //return Netduino.IP.Interop.NetworkInterface.GetNetworkInterfaceCount();
            MethodInfo methodInfo = Type.GetType("Netduino.IP.Interop.NetworkInterface, Netduino.IP.Interop").GetMethod("GetNetworkInterfaceCount", BindingFlags.Public | BindingFlags.Static);
            return (int)methodInfo.Invoke(null, new object[] { });
        }

        private static NetworkInterface GetNetworkInterface(uint interfaceIndex)
        {
            //return (NetworkInterface)Netduino.IP.Interop.NetworkInterface.GetNetworkInterface(interfaceIndex);
            MethodInfo methodInfo = Type.GetType("Netduino.IP.Interop.NetworkInterface, Netduino.IP.Interop").GetMethod("GetNetworkInterface", BindingFlags.Public | BindingFlags.Static);
            return (NetworkInterface)methodInfo.Invoke(null, new object[] { interfaceIndex });
        }

        private void InitializeNetworkInterfaceSettings()
        {
            throw new NotImplementedException();
        }

        private void UpdateConfiguration(int updateType)
        {
            throw new NotImplementedException();
        }

        private static uint IPAddressFromString(string ipAddress)
        {
            throw new NotImplementedException();
			/* TODO: the below code should work...but we need to fix the reference to CC3100 */

            /* NOTE: this code is copy-and-pasted from System.Net.IPAddress.Parse */
            if (ipAddress == null)
                throw new ArgumentNullException();

            ulong ipAddressValue = 0;
            int lastIndex = 0;
            int shiftIndex = 0;
            ulong mask = 0x00000000000000FF;
            ulong octet = 0L;
            int length = ipAddress.Length;

            for (int i = 0; i < length; ++i)
            {
                // Parse to '.' or end of IP address
                if (ipAddress[i] == '.' || i == length - 1)
                    // If the IP starts with a '.'
                    // or a segment is longer than 3 characters or shiftIndex > last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        i = i == length - 1 ? ++i : i;
                        // Int32 stoi32 = Netduino.IP.LinkLayers.CC3100.ConvertStringToInt32(ipAddress.Substring(lastIndex, i - lastIndex)
                        Int32 stoi32 = (Int32)(Type.GetType("Netduino.IP.LinkLayers.CC3100, Netduino.IP.LinkLayers.CC3100").GetMethod("ConvertStringToInt32", BindingFlags.Static).Invoke(null, new object[] { lastIndex, i - lastIndex }));
                        octet = (ulong)(stoi32 & 0x00000000000000FF);
                        ipAddressValue = ipAddressValue + (ulong)((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex = shiftIndex + 8;
                        mask = (mask << 8);
                    }
            }

            return (uint)ipAddressValue;
        }

        private string IPAddressToString(uint ipAddress)
        {
            if(SystemInfo.IsBigEndian)
            {
                return string.Concat(
                                ((ipAddress >> 24) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 0) & 0xFF).ToString()
                                );
            }
            else
            {
                return string.Concat(
                                ((ipAddress >> 0) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 24) & 0xFF).ToString()
                                );
             }
        }

        public void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress)
        {
            try
            {
                _ipAddress = IPAddressFromString(ipAddress);
                _subnetMask = IPAddressFromString(subnetMask);
                _gatewayAddress = IPAddressFromString(gatewayAddress);
                _flags &= ~FLAGS_DHCP;

				/* NOTE: see CC3100 driver for details */
	            throw new NotImplementedException();

                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void EnableDhcp()
        {
            try
            {
                _flags |= FLAGS_DHCP;

				/* NOTE: see CC3100 driver for details */
	            throw new NotImplementedException();

                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void EnableStaticDns(string[] dnsAddresses)
        {
            if (dnsAddresses == null || dnsAddresses.Length == 0 || dnsAddresses.Length > 2)
            {
                throw new ArgumentException();
            }

            uint[] addresses = new uint[2];

            int iAddress = 0;
            for (int i = 0; i < dnsAddresses.Length; i++)
            {
                uint address = IPAddressFromString(dnsAddresses[i]);

                addresses[iAddress] = address;

                if (address != 0)
                {
                    iAddress++;
                }
            }

            try
            {
                _dnsAddress1 = addresses[0];
                _dnsAddress2 = addresses[1];

                _flags &= ~FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void EnableDynamicDns()
        {
            try
            {
                _flags |= FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public string IPAddress
        {
            get 
            { 
			    /* NOTE: see CC3100 driver for details */
                throw new NotImplementedException();

                return IPAddressToString(_ipAddress); 
            }
        }

        public string GatewayAddress
        {
            get 
            { 
			    /* NOTE: see CC3100 driver for details */
                throw new NotImplementedException();

                return IPAddressToString(_gatewayAddress); 
            }
        }

        public string SubnetMask
        {
            get 
            {
                /* NOTE: see CC3100 driver for details */
                throw new NotImplementedException();

                return IPAddressToString(_subnetMask); 
            }
        }

        public bool IsDhcpEnabled
        {
            get { return (_flags & FLAGS_DHCP) != 0; }
        }

        public bool IsDynamicDnsEnabled
        {
            get
            {
                return (_flags & FLAGS_DYNAMIC_DNS) != 0;
            }
        }

        public string[] DnsAddresses
        {
            get
            {
                ArrayList list = new ArrayList();

				/* NOTE: see CC3100 driver for details */
	            throw new NotImplementedException();
                if (_dnsAddress1 != 0)
                {
                    list.Add(IPAddressToString(_dnsAddress1));
                }

                if (_dnsAddress2 != 0)
                {
                    list.Add(IPAddressToString(_dnsAddress2));
                }

                return (string[])list.ToArray(typeof(string));
            }
        }

        private void ReloadSettings()
        {
            Thread.Sleep(100);
            InitializeNetworkInterfaceSettings();
        }

        public void ReleaseDhcpLease()
        {
            try
            {
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void RenewDhcpLease()
        {
            try
            {
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE | UPDATE_FLAGS_DHCP_RENEW);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public byte[] PhysicalAddress
        {
            get { return _macAddress; }
            set
            {
                try
                {
                    _macAddress = value;
                    UpdateConfiguration(UPDATE_FLAGS_MAC);
                }
                finally
                {
                    ReloadSettings();
                }
            }
        }

        public NetworkInterfaceType NetworkInterfaceType
        {
            get { return _networkInterfaceType; }
        }
    }
}

