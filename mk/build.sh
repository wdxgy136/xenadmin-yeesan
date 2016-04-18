#!/bin/sh

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

# This is usually called by Jenkins
# If you define DEBUG environment variable you can make it more verbose.
#
# When are you doing modifications to the build system, always do them in such
# way that it will continue to work even if it's executed manually by a developer
# or from a build automation system.

function check_deps ()
{
  local -a MISSING=()
  for DEP in "$@"
  do
    which $DEP >/dev/null 2>&1
    if [ $? -ne 0 ]; then
      MISSING[${#MISSING}]=${DEP}
    fi
  done
  if [ ${#MISSING} -gt 0 ]; then
    echo "FATAL: One or more build tools were not found in PATH: ${MISSING[@]}"
    exit 1
  fi
}

check_deps nunit-console.exe zip unzip mkisofs wget curl hg git patch mt.exe candle.exe light.exe
if [ "${BUILD_KIND:+$BUILD_KIND}" != production ]
then
    check_deps signtool.exe
fi


if [ -v DEBUG ];
then
  echo "INFO:	DEBUG mode activated (verbose)"
  set -x
fi

set -e

ROOT_DIR="$( cd -P "$( dirname "${BASH_SOURCE[0]}" )/../.." && pwd )"

XENADMIN_DIR="$( cd -P "$( dirname "${BASH_SOURCE[0]}" )/.." && pwd )"
source ${XENADMIN_DIR}/mk/declarations.sh

# if this is an official build 
if [ $get_BUILD_NUMBER -ne 0 ]
then
	cd ${ROOT_DIR}
	#DIR="$( cd -P "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

	# now we are sure we are running from the branch root
	#cd ${DIR}/../..

	if [ -d "xenadmin-ref.hg" ]
	then
	  hg --cwd xenadmin-ref.hg pull -u
	else
	  hg clone ssh://xenhg@hg.uk.xensource.com/carbon/${XS_BRANCH}/xenadmin-ref.hg/
	fi
fi

if test -z "${XC_BRANDING}"; then XC_BRANDING=citrix; fi

git ls-remote git://hg.uk.xensource.com/carbon/${XS_BRANCH}/xenadmin-branding.git &>-
if [ "$?" -eq 0 ]; then
  if [ -d "xenadmin-branding" ]
    then
      rm -rf xenadmin-branding 
  fi
  git clone git://hg.uk.xensource.com/carbon/${XS_BRANCH}/xenadmin-branding.git
  if [ -d ${BRAND_REPO}/${XC_BRANDING} ]; then
	echo "Overwriting Branding folder"
	cp -rf ${BRAND_REPO}/${XC_BRANDING}/* ${REPO}/Branding/
  fi
fi

# overwrite archive-push.sh and push-latest-successful-build.sh files, if they exists in Branding folder
if [ -f ${XENADMIN_DIR}/Branding/branding-archive-push.sh ]; then
  echo "Overwriting mk/archive-push.sh with Branding/branding-archive-push.sh."
  cp ${XENADMIN_DIR}/Branding/branding-archive-push.sh ${XENADMIN_DIR}/mk/archive-push.sh
fi
if [ -f ${XENADMIN_DIR}/Branding/branding-push-latest-successful-build.sh ]; then
  echo "Overwriting mk/push-latest-successful-build.sh with Branding/branding-push-latest-successful-build.sh."
  cp ${XENADMIN_DIR}/Branding/branding-push-latest-successful-build.sh ${XENADMIN_DIR}/mk/push-latest-successful-build.sh
fi

production_jenkins_build()
{
    source ${XENADMIN_DIR}/mk/bumpBuildNumber.sh
    source ${XENADMIN_DIR}/devtools/check-roaming.sh
    source ${XENADMIN_DIR}/devtools/i18ncheck/i18ncheck.sh
    source ${XENADMIN_DIR}/devtools/deadcheck/deadcheck.sh
    source ${XENADMIN_DIR}/devtools/spellcheck/spellcheck.sh
    source ${XENADMIN_DIR}/mk/xenadmin-build.sh
	# Skip the tests if the SKIP_TESTS variable is defined (e.g. in the Jenkins UI, add "export SKIP_TESTS=1" above the call for build script)
	if [ -n "${SKIP_TESTS+x}" ]; then
		echo "Tests skipped because SKIP_TESTS declared"
	else
		source ${XENADMIN_DIR}/mk/tests-checks.sh
	fi
    source ${XENADMIN_DIR}/mk/archive-push.sh
}

# Use this option if you're running on a Jenkins that is not the production Jenkins server
private_jenkins_build()
{
    source ${XENADMIN_DIR}/devtools/spellcheck/spellcheck.sh
    source ${XENADMIN_DIR}/mk/xenadmin-build.sh
    # Skip the tests if the SKIP_TESTS variable is defined (e.g. in the Jenkins UI, add "export SKIP_TESTS=1" above the call for build script)
	if [ -n "${SKIP_TESTS+x}" ]; then
		echo "Tests skipped because SKIP_TESTS declared"
	else
		source ${XENADMIN_DIR}/mk/tests-checks.sh
	fi
}

# Set the PRIVATE_BUILD_MODE variable in order to use the private build mode
# You can do this from the jenkins ui by putting "export PRIVATE_BUILD_MODE=1" above 
# the call for this script

if [ -z "${PRIVATE_BUILD_MODE+xxx}" ]; then
    production_jenkins_build
else
    echo "INFO:	Running private Jenkins build"
    private_jenkins_build
fi
unset PRIVATE_BUILD_MODE
