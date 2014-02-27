using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{

    // wrapper for CpDeviceListUpnpServiceType which moderates device removed notifications if refreshing
    // due to iOs issues with devices all disappearing on refresh (see #829)
    public sealed class ModeratedCpDeviceList : ICpDeviceList
    {
        private CpDeviceList iDeviceList;
        private Action<CpDeviceList, CpDevice> iAdded;
        private Action<CpDeviceList, CpDevice> iRemoved;
        private IModerator iModeratorRefresh;
        private object iLock;
        private Dictionary<string, CpDevice> iDevices;
        private Dictionary<string, CpDevice> iDevicesPendingRemove;
        private bool iRefreshing;
        private const uint kModeratorRefreshTimeout = 3 * 60 * 1000;

        public ModeratedCpDeviceList(string aDomain, string aType, uint aVersion, Action<CpDeviceList, CpDevice> aAdded, Action<CpDeviceList, CpDevice> aRemoved)
        {
            iAdded = aAdded;
            iRemoved = aRemoved;
            iDeviceList = new CpDeviceListUpnpServiceType(aDomain, aType, aVersion, Added, Removed);
            iModeratorRefresh = new ModeratorWaitUntilQuiet(kModeratorRefreshTimeout, ModeratorRefreshExpired);

            iDevices = new Dictionary<string, CpDevice>();
            iDevicesPendingRemove = new Dictionary<string, CpDevice>();
            iLock = new object();
            iRefreshing = false;
        }

        public void Refresh()
        {
            lock (iLock)
            {
                iRefreshing = true;
                iDeviceList.Refresh();
                iModeratorRefresh.Signal();
            }
        }

        private void ModeratorRefreshExpired()
        {
            lock (iLock)
            {
                iRefreshing = false; 
                var pendingRemove = iDevicesPendingRemove.Keys.ToList();
                foreach (var udn in pendingRemove)
                {
                    var device = iDevicesPendingRemove[udn];
                    if (iDevices.ContainsKey(udn))
                    {
                        var origDevice = iDevices[udn];
                        iRemoved(iDeviceList, origDevice);
                        iDevices.Remove(udn);
                        origDevice.RemoveRef();
                    }
                    iDevicesPendingRemove.Remove(udn);
                }
            }
        }


        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            lock (iLock)
            {
                var udn = aDevice.Udn();
                if (iDevicesPendingRemove.ContainsKey(udn))
                {
                    iDevicesPendingRemove.Remove(udn);
                }
                if (!iDevices.ContainsKey(udn))
                {
                    iAdded(iDeviceList, aDevice);
                    aDevice.AddRef();
                    iDevices.Add(udn, aDevice);
                }
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            lock (iLock)
            {
                var udn = aDevice.Udn();
                if (iRefreshing)
                {
                    if (!iDevicesPendingRemove.ContainsKey(udn))
                    {
                        iDevicesPendingRemove.Add(udn, aDevice);
                    }
                }
                else
                {
                    if (iDevices.ContainsKey(udn))
                    {
                        var origDevice = iDevices[udn];
                        iRemoved(iDeviceList, origDevice);
                        iDevices.Remove(udn);
                        origDevice.RemoveRef();
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                foreach (var device in iDevices.Values)
                {
                    device.RemoveRef();
                }
                iDevices.Clear();

                iDevicesPendingRemove.Clear();

                iDeviceList.Dispose();
                iDeviceList = null;

                iModeratorRefresh.Dispose();
                iModeratorRefresh = null;
            }
        }
    }
}
