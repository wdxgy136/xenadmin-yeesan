/*
 * Copyright (c) Citrix Systems, Inc.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 
 *   1) Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 * 
 *   2) Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections;
using System.Collections.Generic;

using CookComputing.XmlRpc;


namespace XenAPI
{
    /// <summary>
    /// VM appliance
    /// First published in XenServer 6.0.
    /// </summary>
    public partial class VM_appliance : XenObject<VM_appliance>
    {
        public VM_appliance()
        {
        }

        public VM_appliance(string uuid,
            string name_label,
            string name_description,
            List<vm_appliance_operation> allowed_operations,
            Dictionary<string, vm_appliance_operation> current_operations,
            List<XenRef<VM>> VMs)
        {
            this.uuid = uuid;
            this.name_label = name_label;
            this.name_description = name_description;
            this.allowed_operations = allowed_operations;
            this.current_operations = current_operations;
            this.VMs = VMs;
        }

        /// <summary>
        /// Creates a new VM_appliance from a Proxy_VM_appliance.
        /// </summary>
        /// <param name="proxy"></param>
        public VM_appliance(Proxy_VM_appliance proxy)
        {
            this.UpdateFromProxy(proxy);
        }

        public override void UpdateFrom(VM_appliance update)
        {
            uuid = update.uuid;
            name_label = update.name_label;
            name_description = update.name_description;
            allowed_operations = update.allowed_operations;
            current_operations = update.current_operations;
            VMs = update.VMs;
        }

        internal void UpdateFromProxy(Proxy_VM_appliance proxy)
        {
            uuid = proxy.uuid == null ? null : (string)proxy.uuid;
            name_label = proxy.name_label == null ? null : (string)proxy.name_label;
            name_description = proxy.name_description == null ? null : (string)proxy.name_description;
            allowed_operations = proxy.allowed_operations == null ? null : Helper.StringArrayToEnumList<vm_appliance_operation>(proxy.allowed_operations);
            current_operations = proxy.current_operations == null ? null : Maps.convert_from_proxy_string_vm_appliance_operation(proxy.current_operations);
            VMs = proxy.VMs == null ? null : XenRef<VM>.Create(proxy.VMs);
        }

        public Proxy_VM_appliance ToProxy()
        {
            Proxy_VM_appliance result_ = new Proxy_VM_appliance();
            result_.uuid = (uuid != null) ? uuid : "";
            result_.name_label = (name_label != null) ? name_label : "";
            result_.name_description = (name_description != null) ? name_description : "";
            result_.allowed_operations = (allowed_operations != null) ? Helper.ObjectListToStringArray(allowed_operations) : new string[] {};
            result_.current_operations = Maps.convert_to_proxy_string_vm_appliance_operation(current_operations);
            result_.VMs = (VMs != null) ? Helper.RefListToStringArray(VMs) : new string[] {};
            return result_;
        }

        /// <summary>
        /// Creates a new VM_appliance from a Hashtable.
        /// </summary>
        /// <param name="table"></param>
        public VM_appliance(Hashtable table)
        {
            uuid = Marshalling.ParseString(table, "uuid");
            name_label = Marshalling.ParseString(table, "name_label");
            name_description = Marshalling.ParseString(table, "name_description");
            allowed_operations = Helper.StringArrayToEnumList<vm_appliance_operation>(Marshalling.ParseStringArray(table, "allowed_operations"));
            current_operations = Maps.convert_from_proxy_string_vm_appliance_operation(Marshalling.ParseHashTable(table, "current_operations"));
            VMs = Marshalling.ParseSetRef<VM>(table, "VMs");
        }

        public bool DeepEquals(VM_appliance other, bool ignoreCurrentOperations)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (!ignoreCurrentOperations && !Helper.AreEqual2(this.current_operations, other.current_operations))
                return false;

            return Helper.AreEqual2(this._uuid, other._uuid) &&
                Helper.AreEqual2(this._name_label, other._name_label) &&
                Helper.AreEqual2(this._name_description, other._name_description) &&
                Helper.AreEqual2(this._allowed_operations, other._allowed_operations) &&
                Helper.AreEqual2(this._VMs, other._VMs);
        }

        public override string SaveChanges(Session session, string opaqueRef, VM_appliance server)
        {
            if (opaqueRef == null)
            {
                Proxy_VM_appliance p = this.ToProxy();
                return session.proxy.vm_appliance_create(session.uuid, p).parse();
            }
            else
            {
                if (!Helper.AreEqual2(_name_label, server._name_label))
                {
                    VM_appliance.set_name_label(session, opaqueRef, _name_label);
                }
                if (!Helper.AreEqual2(_name_description, server._name_description))
                {
                    VM_appliance.set_name_description(session, opaqueRef, _name_description);
                }

                return null;
            }
        }
        /// <summary>
        /// Get a record containing the current state of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static VM_appliance get_record(Session session, string _vm_appliance)
        {
            return new VM_appliance((Proxy_VM_appliance)session.proxy.vm_appliance_get_record(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Get a reference to the VM_appliance instance with the specified UUID.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_uuid">UUID of object to return</param>
        public static XenRef<VM_appliance> get_by_uuid(Session session, string _uuid)
        {
            return XenRef<VM_appliance>.Create(session.proxy.vm_appliance_get_by_uuid(session.uuid, (_uuid != null) ? _uuid : "").parse());
        }

        /// <summary>
        /// Create a new VM_appliance instance, and return its handle.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_record">All constructor arguments</param>
        public static XenRef<VM_appliance> create(Session session, VM_appliance _record)
        {
            return XenRef<VM_appliance>.Create(session.proxy.vm_appliance_create(session.uuid, _record.ToProxy()).parse());
        }

        /// <summary>
        /// Create a new VM_appliance instance, and return its handle.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_record">All constructor arguments</param>
        public static XenRef<Task> async_create(Session session, VM_appliance _record)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_create(session.uuid, _record.ToProxy()).parse());
        }

        /// <summary>
        /// Destroy the specified VM_appliance instance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static void destroy(Session session, string _vm_appliance)
        {
            session.proxy.vm_appliance_destroy(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// Destroy the specified VM_appliance instance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static XenRef<Task> async_destroy(Session session, string _vm_appliance)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_destroy(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Get all the VM_appliance instances with the given label.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_label">label of object to return</param>
        public static List<XenRef<VM_appliance>> get_by_name_label(Session session, string _label)
        {
            return XenRef<VM_appliance>.Create(session.proxy.vm_appliance_get_by_name_label(session.uuid, (_label != null) ? _label : "").parse());
        }

        /// <summary>
        /// Get the uuid field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static string get_uuid(Session session, string _vm_appliance)
        {
            return (string)session.proxy.vm_appliance_get_uuid(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// Get the name/label field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static string get_name_label(Session session, string _vm_appliance)
        {
            return (string)session.proxy.vm_appliance_get_name_label(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// Get the name/description field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static string get_name_description(Session session, string _vm_appliance)
        {
            return (string)session.proxy.vm_appliance_get_name_description(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// Get the allowed_operations field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static List<vm_appliance_operation> get_allowed_operations(Session session, string _vm_appliance)
        {
            return Helper.StringArrayToEnumList<vm_appliance_operation>(session.proxy.vm_appliance_get_allowed_operations(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Get the current_operations field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static Dictionary<string, vm_appliance_operation> get_current_operations(Session session, string _vm_appliance)
        {
            return Maps.convert_from_proxy_string_vm_appliance_operation(session.proxy.vm_appliance_get_current_operations(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Get the VMs field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static List<XenRef<VM>> get_VMs(Session session, string _vm_appliance)
        {
            return XenRef<VM>.Create(session.proxy.vm_appliance_get_vms(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Set the name/label field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_label">New value to set</param>
        public static void set_name_label(Session session, string _vm_appliance, string _label)
        {
            session.proxy.vm_appliance_set_name_label(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_label != null) ? _label : "").parse();
        }

        /// <summary>
        /// Set the name/description field of the given VM_appliance.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_description">New value to set</param>
        public static void set_name_description(Session session, string _vm_appliance, string _description)
        {
            session.proxy.vm_appliance_set_name_description(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_description != null) ? _description : "").parse();
        }

        /// <summary>
        /// Start all VMs in the appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_paused">Instantiate all VMs belonging to this appliance in paused state if set to true.</param>
        public static void start(Session session, string _vm_appliance, bool _paused)
        {
            session.proxy.vm_appliance_start(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", _paused).parse();
        }

        /// <summary>
        /// Start all VMs in the appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_paused">Instantiate all VMs belonging to this appliance in paused state if set to true.</param>
        public static XenRef<Task> async_start(Session session, string _vm_appliance, bool _paused)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_start(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", _paused).parse());
        }

        /// <summary>
        /// Perform a clean shutdown of all the VMs in the appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static void clean_shutdown(Session session, string _vm_appliance)
        {
            session.proxy.vm_appliance_clean_shutdown(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// Perform a clean shutdown of all the VMs in the appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static XenRef<Task> async_clean_shutdown(Session session, string _vm_appliance)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_clean_shutdown(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Perform a hard shutdown of all the VMs in the appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static void hard_shutdown(Session session, string _vm_appliance)
        {
            session.proxy.vm_appliance_hard_shutdown(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// Perform a hard shutdown of all the VMs in the appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static XenRef<Task> async_hard_shutdown(Session session, string _vm_appliance)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_hard_shutdown(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// For each VM in the appliance, try to shut it down cleanly. If this fails, perform a hard shutdown of the VM.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static void shutdown(Session session, string _vm_appliance)
        {
            session.proxy.vm_appliance_shutdown(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse();
        }

        /// <summary>
        /// For each VM in the appliance, try to shut it down cleanly. If this fails, perform a hard shutdown of the VM.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        public static XenRef<Task> async_shutdown(Session session, string _vm_appliance)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_shutdown(session.uuid, (_vm_appliance != null) ? _vm_appliance : "").parse());
        }

        /// <summary>
        /// Assert whether all SRs required to recover this VM appliance are available.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_session_to">The session to which the VM appliance is to be recovered.</param>
        public static void assert_can_be_recovered(Session session, string _vm_appliance, string _session_to)
        {
            session.proxy.vm_appliance_assert_can_be_recovered(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_session_to != null) ? _session_to : "").parse();
        }

        /// <summary>
        /// Assert whether all SRs required to recover this VM appliance are available.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_session_to">The session to which the VM appliance is to be recovered.</param>
        public static XenRef<Task> async_assert_can_be_recovered(Session session, string _vm_appliance, string _session_to)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_assert_can_be_recovered(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_session_to != null) ? _session_to : "").parse());
        }

        /// <summary>
        /// Get the list of SRs required by the VM appliance to recover.
        /// First published in XenServer 6.5.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_session_to">The session to which the list of SRs have to be recovered .</param>
        public static List<XenRef<SR>> get_SRs_required_for_recovery(Session session, string _vm_appliance, string _session_to)
        {
            return XenRef<SR>.Create(session.proxy.vm_appliance_get_srs_required_for_recovery(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_session_to != null) ? _session_to : "").parse());
        }

        /// <summary>
        /// Get the list of SRs required by the VM appliance to recover.
        /// First published in XenServer 6.5.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_session_to">The session to which the list of SRs have to be recovered .</param>
        public static XenRef<Task> async_get_SRs_required_for_recovery(Session session, string _vm_appliance, string _session_to)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_get_srs_required_for_recovery(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_session_to != null) ? _session_to : "").parse());
        }

        /// <summary>
        /// Recover the VM appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_session_to">The session to which the VM appliance is to be recovered.</param>
        /// <param name="_force">Whether the VMs should replace newer versions of themselves.</param>
        public static void recover(Session session, string _vm_appliance, string _session_to, bool _force)
        {
            session.proxy.vm_appliance_recover(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_session_to != null) ? _session_to : "", _force).parse();
        }

        /// <summary>
        /// Recover the VM appliance
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="_vm_appliance">The opaque_ref of the given vm_appliance</param>
        /// <param name="_session_to">The session to which the VM appliance is to be recovered.</param>
        /// <param name="_force">Whether the VMs should replace newer versions of themselves.</param>
        public static XenRef<Task> async_recover(Session session, string _vm_appliance, string _session_to, bool _force)
        {
            return XenRef<Task>.Create(session.proxy.async_vm_appliance_recover(session.uuid, (_vm_appliance != null) ? _vm_appliance : "", (_session_to != null) ? _session_to : "", _force).parse());
        }

        /// <summary>
        /// Return a list of all the VM_appliances known to the system.
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static List<XenRef<VM_appliance>> get_all(Session session)
        {
            return XenRef<VM_appliance>.Create(session.proxy.vm_appliance_get_all(session.uuid).parse());
        }

        /// <summary>
        /// Get all the VM_appliance Records at once, in a single XML RPC call
        /// First published in XenServer 6.0.
        /// </summary>
        /// <param name="session">The session</param>
        public static Dictionary<XenRef<VM_appliance>, VM_appliance> get_all_records(Session session)
        {
            return XenRef<VM_appliance>.Create<Proxy_VM_appliance>(session.proxy.vm_appliance_get_all_records(session.uuid).parse());
        }

        /// <summary>
        /// Unique identifier/object reference
        /// </summary>
        public virtual string uuid
        {
            get { return _uuid; }
            set
            {
                if (!Helper.AreEqual(value, _uuid))
                {
                    _uuid = value;
                    Changed = true;
                    NotifyPropertyChanged("uuid");
                }
            }
        }
        private string _uuid;

        /// <summary>
        /// a human-readable name
        /// </summary>
        public virtual string name_label
        {
            get { return _name_label; }
            set
            {
                if (!Helper.AreEqual(value, _name_label))
                {
                    _name_label = value;
                    Changed = true;
                    NotifyPropertyChanged("name_label");
                }
            }
        }
        private string _name_label;

        /// <summary>
        /// a notes field containing human-readable description
        /// </summary>
        public virtual string name_description
        {
            get { return _name_description; }
            set
            {
                if (!Helper.AreEqual(value, _name_description))
                {
                    _name_description = value;
                    Changed = true;
                    NotifyPropertyChanged("name_description");
                }
            }
        }
        private string _name_description;

        /// <summary>
        /// list of the operations allowed in this state. This list is advisory only and the server state may have changed by the time this field is read by a client.
        /// </summary>
        public virtual List<vm_appliance_operation> allowed_operations
        {
            get { return _allowed_operations; }
            set
            {
                if (!Helper.AreEqual(value, _allowed_operations))
                {
                    _allowed_operations = value;
                    Changed = true;
                    NotifyPropertyChanged("allowed_operations");
                }
            }
        }
        private List<vm_appliance_operation> _allowed_operations;

        /// <summary>
        /// links each of the running tasks using this object (by reference) to a current_operation enum which describes the nature of the task.
        /// </summary>
        public virtual Dictionary<string, vm_appliance_operation> current_operations
        {
            get { return _current_operations; }
            set
            {
                if (!Helper.AreEqual(value, _current_operations))
                {
                    _current_operations = value;
                    Changed = true;
                    NotifyPropertyChanged("current_operations");
                }
            }
        }
        private Dictionary<string, vm_appliance_operation> _current_operations;

        /// <summary>
        /// all VMs in this appliance
        /// </summary>
        public virtual List<XenRef<VM>> VMs
        {
            get { return _VMs; }
            set
            {
                if (!Helper.AreEqual(value, _VMs))
                {
                    _VMs = value;
                    Changed = true;
                    NotifyPropertyChanged("VMs");
                }
            }
        }
        private List<XenRef<VM>> _VMs;
    }
}
