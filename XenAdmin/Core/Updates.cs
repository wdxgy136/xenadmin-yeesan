﻿/* Copyright (c) Citrix Systems Inc. 
 * All rights reserved. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using XenAdmin.Actions;
using XenAPI;
using XenAdmin.Alerts;
using XenAdmin.Network;


namespace XenAdmin.Core
{
    public class Updates
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static event Action<bool, string> CheckForUpdatesCompleted;
        public static event Action CheckForUpdatesStarted;

        private static readonly object downloadedUpdatesLock = new object();
        private static List<XenServerVersion> XenServerVersionsForAutoCheck = new List<XenServerVersion>();
        private static List<XenServerPatch> XenServerPatches = new List<XenServerPatch>();
        private static List<XenCenterVersion> XenCenterVersions = new List<XenCenterVersion>();
        private static List<XenServerVersion> XenServerVersions = new List<XenServerVersion>();

        private static readonly object updateAlertsLock = new object();
        private static readonly ChangeableList<Alert> updateAlerts = new ChangeableList<Alert>();
        
        public static IEnumerable<Alert> UpdateAlerts
        {
            get { return updateAlerts; }
        }

        public static int UpdateAlertsCount
        {
            get { return updateAlerts.Count; }
        }

        private static void AddUpate(Alert update)
        {
            try
            {
                lock (updateAlertsLock)
                {
                    if(!updateAlerts.Contains(update))
                    {
                        updateAlerts.Add(update);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Failed to add update", e);
            }
        }

        public static void RemoveUpdate(Alert update)
        {
            try
            {
                lock (updateAlertsLock)
                {
                    if(updateAlerts.Contains(update))
                    {
                        updateAlerts.Remove(update);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Failed to remove update", e);
            }
        }

        /// <summary>
        /// Dismisses the updates in the given list i.e. they are added in the 
        /// other_config list of each pool and removed from the Updates.UpdateAlerts list.
        /// </summary>
        /// <param name="toBeDismissed"></param>
        public static void DismissUpdates(List<Alert> toBeDismissed)
        {
            if (toBeDismissed.Count == 0)
                return;

            foreach(IXenConnection connection in ConnectionsManager.XenConnectionsCopy)
            {
                if (!Alert.AllowedToDismiss(connection))
                    continue;

                XenAPI.Pool pool = Helpers.GetPoolOfOne(connection);
                if (pool == null)
                    continue;

                Dictionary<string, string> other_config = pool.other_config;

                foreach (Alert alert in toBeDismissed)
                {
                  
                    if (alert is XenServerPatchAlert)
                    {

                        if (other_config.ContainsKey(IgnorePatchAction.IgnorePatchKey))
                        {
                            List<string> current = new List<string>(other_config[IgnorePatchAction.IgnorePatchKey].Split(','));
                            if (current.Contains(((XenServerPatchAlert)alert).Patch.Uuid, StringComparer.OrdinalIgnoreCase))
                                continue;
                            current.Add(((XenServerPatchAlert)alert).Patch.Uuid);
                            other_config[IgnorePatchAction.IgnorePatchKey] = string.Join(",", current.ToArray());
                        }
                        else
                        {
                            other_config.Add(IgnorePatchAction.IgnorePatchKey, ((XenServerPatchAlert)alert).Patch.Uuid);
                        }
                    }
                    if (alert is XenServerVersionAlert)
                    {

                        if (other_config.ContainsKey(IgnoreServerAction.LAST_SEEN_SERVER_VERSION_KEY))
                        {
                            List<string> current = new List<string>(other_config[IgnoreServerAction.LAST_SEEN_SERVER_VERSION_KEY].Split(','));
                            if (current.Contains(((XenServerVersionAlert)alert).Version.VersionAndOEM))
                                continue;
                            current.Add(((XenServerVersionAlert)alert).Version.VersionAndOEM);
                            other_config[IgnoreServerAction.LAST_SEEN_SERVER_VERSION_KEY] = string.Join(",", current.ToArray());
                        }
                        else
                        {
                            other_config.Add(IgnoreServerAction.LAST_SEEN_SERVER_VERSION_KEY, ((XenServerVersionAlert)alert).Version.VersionAndOEM);
                        }                       
                    }
                    Updates.RemoveUpdate(alert);
                }

                XenAPI.Pool.set_other_config(connection.Session, pool.opaque_ref, other_config);
            }
        }

        private static Alert FindUpdate(Alert alert)
        {
            lock (updateAlertsLock)
                return FindUpdate(a => a.Equals(alert));
        }

        private static Alert FindUpdate(Predicate<Alert> predicate)
        {
            lock (updateAlertsLock)
                return updateAlerts.Find(predicate);
        }


        /// <summary>
        /// If AutomaticCheck is enabled it checks for updates regardless the
        /// value of the parameter force. If AutomaticCheck is disabled it only
        /// checks if force is true.
        /// </summary>
        public static void CheckForUpdates(bool force)
        {
            if (Helpers.CommonCriteriaCertificationRelease)
                return;

            if (Properties.Settings.Default.AllowXenCenterUpdates ||
                Properties.Settings.Default.AllowXenServerUpdates ||
                Properties.Settings.Default.AllowPatchesUpdates || force)
            {
                DownloadUpdatesXmlAction action = new DownloadUpdatesXmlAction(
                    Properties.Settings.Default.AllowXenCenterUpdates || force,
                    Properties.Settings.Default.AllowXenServerUpdates || force,
                    Properties.Settings.Default.AllowPatchesUpdates || force,
                    Branding.CheckForUpdatesUrl);
                {
                    action.Completed += actionCompleted;
                }

                if (CheckForUpdatesStarted != null)
                    CheckForUpdatesStarted();

                action.RunAsync();
            }
        }
        
        private static void actionCompleted(ActionBase sender)
        {
            Program.AssertOffEventThread();

            DownloadUpdatesXmlAction action = sender as DownloadUpdatesXmlAction;
            if (action == null)
                return;

            bool succeeded = action.Succeeded;
            string errorMessage = string.Empty;

            lock (updateAlertsLock)
                updateAlerts.Clear();

            if (succeeded)
            {
                lock (downloadedUpdatesLock)
                {
                    var xcvs = action.XenCenterVersions.Where(v => !XenCenterVersions.Contains(v));
                    XenCenterVersions.AddRange(xcvs);

                    var versForAutoCheck = action.XenServerVersionsForAutoCheck.Where(v => !XenServerVersionsForAutoCheck.Contains(v));
                    XenServerVersionsForAutoCheck.AddRange(versForAutoCheck);

                    var vers = action.XenServerVersions.Where(v => !XenServerVersions.Contains(v));
                    XenServerVersions.AddRange(vers);

                    var patches = action.XenServerPatches.Where(p => !XenServerPatches.Contains(p));
                    XenServerPatches.AddRange(patches);
                }

                var xenCenterAlert = NewXenCenterUpdateAlert(XenCenterVersions, Program.Version);
                if (xenCenterAlert != null && !xenCenterAlert.IsDismissed())
                    updateAlerts.Add(xenCenterAlert);

                var xenServerUpdateAlert = NewXenServerVersionAlert(XenServerVersionsForAutoCheck);
                if (xenServerUpdateAlert != null && !xenServerUpdateAlert.CanIgnore)
                    updateAlerts.Add(xenServerUpdateAlert);

                var xenServerPatchAlerts = NewXenServerPatchAlerts(XenServerVersions, XenServerPatches);
                if (xenServerPatchAlerts != null)
                    updateAlerts.AddRange(xenServerPatchAlerts.Where(alert => !alert.CanIgnore));
            }
            else
            {
                if (action.Exception != null)
                {
                    if (action.Exception is System.Net.Sockets.SocketException)
                    {
                        errorMessage = Messages.AVAILABLE_UPDATES_NETWORK_ERROR;
                    }
                    else
                    {
                        // Clean up and remove excess newlines, carriage returns, trailing nonsense
                        string errorText = action.Exception.Message.Trim();
                        errorText = System.Text.RegularExpressions.Regex.Replace(errorText, @"\r\n+", "");
                        errorMessage = string.Format(Messages.AVAILABLE_UPDATES_ERROR, errorText);
                    }
                }

                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = Messages.AVAILABLE_UPDATES_INTERNAL_ERROR;
            }

            if (CheckForUpdatesCompleted != null)
                CheckForUpdatesCompleted(succeeded, errorMessage);
        }


        public static void RegisterCollectionChanged(CollectionChangeEventHandler handler)
        {
            updateAlerts.CollectionChanged += handler;
        }

        public static void DeregisterCollectionChanged(CollectionChangeEventHandler handler)
        {
            updateAlerts.CollectionChanged -= handler;
        }


        public static XenCenterUpdateAlert NewXenCenterUpdateAlert(List<XenCenterVersion> xenCenterVersions, Version currentProgramVersion)
        {
            if (Helpers.CommonCriteriaCertificationRelease)
                return null;

            XenCenterVersion toUse = null;
            if (xenCenterVersions.Count != 0 && currentProgramVersion != new Version(0, 0, 0, 0))
            {
                var latest = from v in xenCenterVersions where v.IsLatest select v;

                toUse = latest.FirstOrDefault(xcv => xcv.Lang == Program.CurrentLanguage) ??
                        latest.FirstOrDefault(xcv => string.IsNullOrEmpty(xcv.Lang));
            }

            if (toUse == null)
                return null;

            if (toUse.Version > currentProgramVersion ||
                (toUse.Version == currentProgramVersion && toUse.Lang == Program.CurrentLanguage &&
                 !PropertyManager.IsCultureLoaded(Program.CurrentCulture)))
            {
                return new XenCenterUpdateAlert(toUse);
            }

            log.Info(string.Format("Not alerting XenCenter update - lastest = {0}, detected = {1}",
                                   toUse.VersionAndLang, Program.VersionAndLanguage));
            return null;
        }

        public static List<XenServerPatchAlert> NewXenServerPatchAlerts(List<XenServerVersion> xenServerVersions,
            List<XenServerPatch> xenServerPatches)
        {
            if (Helpers.CommonCriteriaCertificationRelease)
                return null;

            var alerts = new List<XenServerPatchAlert>();

            foreach (IXenConnection xenConnection in ConnectionsManager.XenConnectionsCopy)
            {
                Host master = Helpers.GetMaster(xenConnection);
                Pool pool = Helpers.GetPoolOfOne(xenConnection);
                List<Host> hosts = xenConnection.Cache.Hosts.ToList();
                if (master == null || pool == null)
                    continue;

                var serverVersions = xenServerVersions.FindAll(version =>
                                                  {
                                                      if (version.BuildNumber != string.Empty)
                                                          return (master.BuildNumberRaw == version.BuildNumber);

                                                      return Helpers.HostProductVersionWithOEM(master) == version.VersionAndOEM
                                                             || (version.Oem != null && Helpers.OEMName(master).StartsWith(version.Oem)
                                                                 && Helpers.HostProductVersion(master) == version.Version.ToString());
                                                  });

                if (serverVersions.Count == 0)
                    continue;

                foreach (XenServerVersion xenServerVersion in serverVersions)
                {
                    XenServerVersion version = xenServerVersion;
                    List<XenServerPatch> patches = xenServerPatches.FindAll(patch => version.Patches.Contains(patch));

                    if (patches.Count == 0)
                        continue;

                    foreach (XenServerPatch xenServerPatch in patches)
                    {
                        var alert = new XenServerPatchAlert(xenServerPatch);
                        var existingAlert = alerts.Find(al => al.Equals(alert));

                        if (existingAlert != null)
                            alert = existingAlert;
                        else
                            alerts.Add(alert);

                        if (!xenConnection.IsConnected)
                            continue;

                        XenServerPatch serverPatch = xenServerPatch;

                        // A patch can be installed on a host if:
                        // 1. it is not already installed and
                        // 2. the host has all the required patches installed and
                        // 3. the host doesn't have any of the conflicting patches installed

                        var noPatchHosts = hosts.Where(host =>
                            {
                                var appliedPatches = host.AppliedPatches();
                                // 1. patch is not already installed 
                                if (appliedPatches.Any(patch => string.Equals(patch.uuid, serverPatch.Uuid, StringComparison.OrdinalIgnoreCase)))
                                    return false;

                                // 2. the host has all the required patches installed
                                if (serverPatch.RequiredPatches != null && serverPatch.RequiredPatches.Count > 0 &&
                                    !serverPatch.RequiredPatches.All(requiredPatchUuid => appliedPatches.Any(patch => string.Equals(patch.uuid, requiredPatchUuid, StringComparison.OrdinalIgnoreCase))))
                                    return false;

                                // 3. the host doesn't have any of the conflicting patches installed
                                if (serverPatch.ConflictingPatches != null && serverPatch.ConflictingPatches.Count > 0 &&
                                    serverPatch.ConflictingPatches.Any(conflictingPatchUuid => appliedPatches.Any(patch => string.Equals(patch.uuid, conflictingPatchUuid, StringComparison.OrdinalIgnoreCase))))
                                    return false;

                                return true;
                            });

                        if (noPatchHosts.Count() == hosts.Count)
                            alert.IncludeConnection(xenConnection);
                        else
                            alert.IncludeHosts(noPatchHosts);
                    }
                }
            }

            return alerts;
        }

        public static XenServerVersionAlert NewXenServerVersionAlert(List<XenServerVersion> xenServerVersions)
        {
            if (Helpers.CommonCriteriaCertificationRelease)
                return null;

            var latestVersion = xenServerVersions.FindAll(item => item.Latest).OrderByDescending(v => v.Version).FirstOrDefault();
            if (latestVersion == null)
                return null;

            var alert = new XenServerVersionAlert(latestVersion);

            foreach (IXenConnection xc in ConnectionsManager.XenConnectionsCopy)
            {
                if (!xc.IsConnected)
                    continue;

                Host master = Helpers.GetMaster(xc);
                Pool pool = Helpers.GetPoolOfOne(xc);
                List<Host> hosts = xc.Cache.Hosts.ToList();
                if (master == null || pool == null)
                    continue;

                var outOfDateHosts = hosts.Where(host => new Version(Helpers.HostProductVersion(host)) < latestVersion.Version);

                if (outOfDateHosts.Count() == hosts.Count)
                    alert.IncludeConnection(xc);
                else
                    alert.IncludeHosts(outOfDateHosts);
            }

            return alert;
        }


        public static void CheckServerVersion()
        {
            var alert = NewXenServerVersionAlert(XenServerVersionsForAutoCheck);
            if (alert == null)
                return;

            CheckUpdate(alert);
        }

        public static void CheckServerPatches()
        {
            var alerts = NewXenServerPatchAlerts(XenServerVersions, XenServerPatches);
            if (alerts == null)
                return;

            foreach (var alert in alerts)
                CheckUpdate(alert);
        }

        private static void CheckUpdate(XenServerUpdateAlert alert)
        {
            var existingAlert = FindUpdate(alert);

            if (existingAlert != null && alert.CanIgnore)
                RemoveUpdate(existingAlert);
            else if (existingAlert != null)
                ((XenServerUpdateAlert)existingAlert).CopyConnectionsAndHosts(alert);
            else if (!alert.CanIgnore)
                AddUpate(alert);
        }

        public static void RestoreDismissedUpdates()
        {
            foreach (IXenConnection _connection in ConnectionsManager.XenConnectionsCopy)
            {
                if (!AllowedToRestoreDismissedUpdates(_connection))
                    continue;

                XenAPI.Pool pool = Helpers.GetPoolOfOne(_connection);
                if (pool == null)
                    continue;

                Dictionary<string, string> other_config = pool.other_config;

                if (other_config.ContainsKey(IgnorePatchAction.IgnorePatchKey))
                {
                    other_config.Remove(IgnorePatchAction.IgnorePatchKey);
                }
                if (other_config.ContainsKey(IgnoreServerAction.LAST_SEEN_SERVER_VERSION_KEY))
                {
                    other_config.Remove(IgnoreServerAction.LAST_SEEN_SERVER_VERSION_KEY);
                }

                XenAPI.Pool.set_other_config(_connection.Session, pool.opaque_ref, other_config);
            }

            Properties.Settings.Default.LatestXenCenterSeen = "";
            Settings.TrySaveSettings();

            Updates.CheckForUpdates(true);
        }

        /// <summary>
        /// Checks the user has sufficient RBAC privileges to restore dismissed alerts on a given connection
        /// </summary>
        public static bool AllowedToRestoreDismissedUpdates(IXenConnection c)
        {
            if (c == null || c.Session == null)
                return false;

            if (c.Session.IsLocalSuperuser)
                return true;

            List<Role> rolesAbleToCompleteAction = Role.ValidRoleList("Pool.set_other_config", c);
            foreach (Role possibleRole in rolesAbleToCompleteAction)
            {
                if (c.Session.Roles.Contains(possibleRole))
                    return true;
            }
            return false;
        }
    }
}
