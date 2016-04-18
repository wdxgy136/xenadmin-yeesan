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
using System.Threading;
using XenAdmin.Network;
using XenAPI;


namespace XenAdmin.Actions
{
    /// <summary>
    /// ParallelAction takes a list of any number of actions and runs a certain number of them simultaneously.
    /// Once one simultaneous action is finished the next one in the queue is started until all are complete
    /// </summary>
    public class ParallelAction : MultipleAction
    {
        private const int DEFAULT_MAX_NUMBER_OF_PARALLEL_ACTIONS = 25;

        //Change parameter to increase the number of concurrent actions running
        private readonly ProduceConsumerQueue _queuePC;

        public ParallelAction (IXenConnection connection, string title, string startDescription, string endDescription, List<AsyncAction> subActions)
            : base(connection, title, startDescription, endDescription, subActions)
        {
            _queuePC = new ProduceConsumerQueue(Math.Min(DEFAULT_MAX_NUMBER_OF_PARALLEL_ACTIONS, subActions.Count));
        }

        public ParallelAction(IXenConnection connection, string title, string startDescription, string endDescription, List<AsyncAction> subActions, int maxNumberOfParallelActions)
            : base(connection, title, startDescription, endDescription, subActions)
        {
            _queuePC = new ProduceConsumerQueue(Math.Min(maxNumberOfParallelActions, subActions.Count));
        }

        protected override void RunSubActions(List<Exception> exceptions)
        {
            foreach (AsyncAction subAction in subActions)
            {
                AsyncAction action = subAction;
                action.Completed += action_Completed;
                _queuePC.EnqueueItem(
                    () =>
                    {
                        try
                        {
                            action.RunExternal(Session);
                        }
                        catch (Exception e)
                        {
                            Failure f = e as Failure;
                            if (f != null && Connection != null &&
                                f.ErrorDescription[0] == Failure.RBAC_PERMISSION_DENIED)
                            {
                                Failure.ParseRBACFailure(f, Connection, Session ?? Connection.Session);
                            }
                            exceptions.Add(e);
                            // Record the first exception we come to. Though later if there are more than one we will replace this with non specific one.
                            if (Exception == null)
                                Exception = e;
                        }
                    });

            }
            lock (_lock)
            {
                Monitor.Wait(_lock);
            }
        }

        private readonly object _lock = new object();
        private volatile int i = 0;
        
        void action_Completed(ActionBase sender)
        {
            lock (_lock)
            {
                i++;
                PercentComplete = 100 * i / subActions.Count;
                if (i == subActions.Count)
                    Monitor.Pulse(_lock);
            }
        }

        protected override void MultipleAction_Completed(ActionBase sender)
        {
            
            base.MultipleAction_Completed(sender);
            _queuePC.StopWorkers(false);
        }
    }
}
