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
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.IO;
using XenCenterLib.Archive;

namespace XenAdmin.Actions
{
    internal enum DownloadState { InProgress, Cancelled, Completed, Error };

    public class DownloadAndUnzipXenServerPatchAction : AsyncAction
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int SLEEP_TIME = 900;

        private readonly Uri address;
        private readonly string outFileName;
        private readonly string updateName;
        private readonly string updateFileExtension;
        private DownloadState patchDownloadState;
        private Exception patchDownloadError;

        public string PatchPath
        {
            get; private set;
        }

        public DownloadAndUnzipXenServerPatchAction(string patchName, Uri uri, string outputFileName)
            : this(patchName, uri, outputFileName, InvisibleMessages.XEN_UPDATE)
        { }

        public DownloadAndUnzipXenServerPatchAction(string patchName, Uri uri, string outputFileName, string updateFileExtension)
            : base(null, string.Format(Messages.DOWNLOAD_AND_EXTRACT_ACTION_TITLE, patchName), string.Empty, false)
        {
            updateName = patchName;
            address = uri;
            outFileName = outputFileName;
            this.updateFileExtension = updateFileExtension;
        }

        private void DownloadFile()
        {
            WebClient client = new WebClient();
            //register download events
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            //start the download
            client.DownloadFileAsync(address, outFileName);

            patchDownloadState = DownloadState.InProgress;
            bool patchDownloadCancelling = false;

            //wait for the file to be downloaded
            while (patchDownloadState == DownloadState.InProgress)
            {
                if (!patchDownloadCancelling && (Cancelling || Cancelled))
                {
                    Description = Messages.DOWNLOAD_AND_EXTRACT_ACTION_DOWNLOAD_CANCELLED_DESC;
                    client.CancelAsync();
                    patchDownloadCancelling = true;
                }
                Thread.Sleep(SLEEP_TIME);
            }

            //deregister download events
            client.DownloadProgressChanged -= client_DownloadProgressChanged;
            client.DownloadFileCompleted -= client_DownloadFileCompleted;

            if (patchDownloadState == DownloadState.Cancelled)
                throw new CancelledException();

            if (patchDownloadState == DownloadState.Error)
                MarkCompleted(patchDownloadError ?? new Exception(Messages.ERROR_UNKNOWN));
        }

        private void ExtractFile()
        {
            ArchiveIterator iterator = null;
            try
            {
                using (Stream stream = new FileStream(outFileName, FileMode.Open, FileAccess.Read))
                {
                    iterator = ArchiveFactory.Reader(ArchiveFactory.Type.Zip, stream);
                    DotNetZipZipIterator zipIterator = iterator as DotNetZipZipIterator;
                    if (zipIterator != null)
                    {
                        zipIterator.CurrentFileExtractProgressChanged +=
                            archiveIterator_CurrentFileExtractProgressChanged;
                    }

                    while (iterator.HasNext())
                    {
                        if (Path.GetExtension(iterator.CurrentFileName()) == "." + updateFileExtension)
                        {
                            string path = Path.Combine(Path.GetDirectoryName(outFileName), iterator.CurrentFileName());

                            using (Stream outputStream = new FileStream(path, FileMode.Create))
                            {
                                iterator.ExtractCurrentFile(outputStream);
                                PatchPath = path;
                                break;
                            }
                        }
                    }

                    if (zipIterator != null)
                    {
                        zipIterator.CurrentFileExtractProgressChanged -=
                            archiveIterator_CurrentFileExtractProgressChanged;
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception occurred when extracting downloaded archive: {0}", e.Message);
                throw new Exception(Messages.DOWNLOAD_AND_EXTRACT_ACTION_EXTRACTING_ERROR);
            }
            finally
            {
                if (iterator != null)
                    iterator.Dispose();
                File.Delete(outFileName);
            }
            
            if (string.IsNullOrEmpty(PatchPath))
            {
                MarkCompleted(new Exception(Messages.DOWNLOAD_AND_EXTRACT_ACTION_FILE_NOT_FOUND));
                log.DebugFormat("File '{0}.{1}' could not be located in downloaded archive", updateName, updateFileExtension);
            }
        }

        protected override void Run()
        {
            log.DebugFormat("Downloading XenServer patch '{0}' (url: {1})", updateName, address);

            Description = string.Format(Messages.DOWNLOAD_AND_EXTRACT_ACTION_DOWNLOADING_DESC, updateName);
            LogDescriptionChanges = false;
            DownloadFile();
            LogDescriptionChanges = true;

            if (IsCompleted || Cancelled)
                return;

            if (Cancelling)
                throw new CancelledException();

            log.DebugFormat("Extracting XenServer patch '{0}'", updateName);
            Description = string.Format(Messages.DOWNLOAD_AND_EXTRACT_ACTION_EXTRACTING_DESC, updateName);
            ExtractFile();
            log.DebugFormat("Extracting XenServer patch '{0}' completed", updateName);

            Description = Messages.COMPLETED;
            MarkCompleted();
        }

        void archiveIterator_CurrentFileExtractProgressChanged(object sender, ExtractProgressChangedEventArgs e)
        {
            int pc = 95 + (int)(5.0 * e.BytesTransferred / e.TotalBytesToTransfer);
            if (pc != PercentComplete)
                PercentComplete = pc;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int pc = (int)(95.0 * e.BytesReceived / e.TotalBytesToReceive);
            if (pc != PercentComplete)
            {
                PercentComplete = pc;
                Description = string.Format(Messages.DOWNLOAD_AND_EXTRACT_ACTION_DOWNLOADING_DETAILS_DESC, updateName,
                                            Util.DiskSizeString(e.BytesReceived),
                                            Util.DiskSizeString(e.TotalBytesToReceive));
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) //user cancelled
            {
                patchDownloadState = DownloadState.Cancelled;
                log.DebugFormat("XenServer patch '{0}' download cancelled by the user", updateName);
                return;
            }

            if (e.Error != null) //failure
            {
                patchDownloadError = e.Error;
                log.DebugFormat("XenServer patch '{0}' download failed", updateName);
                patchDownloadState = DownloadState.Error;
                return;
            }

            //success
            patchDownloadState = DownloadState.Completed;
            log.DebugFormat("XenServer patch '{0}' download completed successfully", updateName);
        }

        public override void RecomputeCanCancel()
        {
            CanCancel = !Cancelling && !IsCompleted && (patchDownloadState == DownloadState.InProgress);
        }

        protected override void CancelRelatedTask()
        {
        }
    }
}