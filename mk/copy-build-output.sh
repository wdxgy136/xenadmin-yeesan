# Copyright (c) Citrix Systems Inc. 
# All rights reserved.
# 
# Redistribution and use in source and binary forms, 
# with or without modification, are permitted provided 
# that the following conditions are met: 
# 
# *   Redistributions of source code must retain the above 
#     copyright notice, this list of conditions and the 
#     following disclaimer. 
# *   Redistributions in binary form must reproduce the above 
#     copyright notice, this list of conditions and the 
#     following disclaimer in the documentation and/or other 
#     materials provided with the distribution. 
# 
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
# CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
# INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
# MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
# DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
# CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
# SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
# BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
# SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
# WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
# NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
# OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
# SUCH DAMAGE.

set -eu

# Secure build: update buildtools, copy output to local disk, then to remote.
cd ${OUTPUT_DIR}
if [ "${BUILD_KIND:+$BUILD_KIND}" = production ]
then
	 echo "Secure build: update buildtools, copy output to local disk, then to remote."
     $STORE_FILES remoteupdate xensb.uk.xensource.com xenbuild git://hg.uk.xensource.com/closed/windows buildtools.git /usr/groups/build/windowsbuilds
     $STORE_FILES store $SECURE_BUILD_ARCHIVE_UNC $get_JOB_NAME $BUILD_NUMBER *
     $STORE_FILES remotestore xensb.uk.xensource.com xenbuild /usr/groups/build/windowsbuilds buildtools.git /usr/groups/build/windowsbuilds/WindowsBuilds $SECURE_BUILD_ARCHIVE_UNC $get_JOB_NAME $BUILD_NUMBER *
fi