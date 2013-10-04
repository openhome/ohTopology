using System;
using System.Collections.Generic;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    /// <summary>
    /// Tracks CpDevices to ensure they are properly cleaned up
    /// when the device disappears.
    /// </summary>
    class CpDeviceTracker : IDisposable
    {
        class DeviceEntry
        {
            readonly CpDevice iCpDevice;
            readonly Action iCleanupAction;
            bool iDisposed;

            public DeviceEntry(CpDevice aCpDevice, Action aCleanupAction)
            {
                iCpDevice = aCpDevice;
                iCleanupAction = aCleanupAction;
            }

            public void Dispose()
            {
                if (iDisposed) return;
                iDisposed = true;
                iCleanupAction();
                iCpDevice.RemoveRef();
            }
        }

        private readonly Dictionary<string, DeviceEntry> iCpDeviceLookup = new Dictionary<string, DeviceEntry>();

        /// <summary>
        /// Add a CpDevice to the table. It's not safe to call this
        /// if the device might already be in the table.
        /// </summary>
        /// <param name="aCpDevice">The CpDevice.</param>
        /// <param name="aCleanupAction">
        /// An action to invoke when the device is removed. This will
        /// be invoked synchronously during either DestroyDeviceByUdn
        /// or Dispose.
        /// </param>
        public void AddDevice(CpDevice aCpDevice, Action aCleanupAction)
        {
            aCpDevice.AddRef();
            if (iCpDeviceLookup.ContainsKey(aCpDevice.Udn()))
            {
                throw new ArgumentException("Device is already in table.");
            }
            iCpDeviceLookup.Add(aCpDevice.Udn(), new DeviceEntry(aCpDevice, aCleanupAction));
        }

        /// <summary>
        /// Search the table for the device with the given UDN, cleaning
        /// it up if it's present. It's safe to call this when the item
        /// is not in the table.
        /// </summary>
        /// <param name="aUdn">
        /// The UDN of the device.
        /// </param>
        /// <returns></returns>
        public bool DestroyDeviceByUdn(string aUdn)
        {
            DeviceEntry deviceEntry;
            if (iCpDeviceLookup.TryGetValue(aUdn, out deviceEntry))
            {
                iCpDeviceLookup.Remove(aUdn);
                deviceEntry.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dispose all the CpDevices and invoke the cleanup actions.
        /// </summary>
        public void Dispose()
        {
            foreach (var deviceEntry in iCpDeviceLookup.Values)
            {
                deviceEntry.Dispose();
            }
            iCpDeviceLookup.Clear();
        }
    }
}