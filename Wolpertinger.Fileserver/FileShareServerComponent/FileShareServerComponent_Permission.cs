/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
	in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products 
	derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Fileserver
{
    public partial class FileShareServerComponent
    {
        /// <summary>
        /// Server implementation of the AddPermission RemoteMethod
        /// </summary>
        /// <param name="p">The permission to be added</param>
        [TrustLevel(4)]
        [MethodCallHandler(FileShareMethods.AddPermission)]
        public CallResult AddPermission(Permission p)
        {
            logger.Info("Adding Permission {0}", p);

            if (!permissions.ContainsKey(p.Path.ToLower()))
            {
                permissions.Add(p.Path.ToLower(), p);
            }
            else
            {
                permissions[p.Path.ToLower()] = p;
            }
            savePermissions();

            return new VoidResult();
        }


        /// <summary>
        /// Server implementation of the RemovePermission RemoteMethod
        /// </summary>
        /// <param name="path">The path to remove the permission from</param>
        [TrustLevel(4)]
        [MethodCallHandler(FileShareMethods.RemovePermission)]
        public CallResult RemovePermission(string path)
        {
            logger.Info("Removing Permission for {0}", path);

            path = path.ToLower();
            if (permissions.ContainsKey(path))
            {
                permissions.Remove(path);
                savePermissions();
                logger.Info("Permission removed");
                return new VoidResult();
            }
            else
            {
                logger.Error("No explicit permission found");
                //send error, if permission could not be found
                return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
            }
        }



        /// <summary>
        /// Server implementation of the GetPermission RemoteMethod
        /// </summary>
        /// <param name="path">The path to checks the calling client's permission rights for</param>
        [TrustLevel(3)]
        [MethodCallHandler(FileShareMethods.GetPermission)]
        public CallResult GetPermission(string path)
        {
            logger.Info("Permission for {0} requested", path);
            return new ResponseResult(checkPermission(path, permissions));
        }

        /// <summary>
        /// Server implementation of the GetAddedPermissions RemoteMethod
        /// </summary>
        [TrustLevel(4)]
        [MethodCallHandler(FileShareMethods.GetAddedPermissions)]
        public CallResult GetAddedPermissions()
        {
            logger.Info("All Permission requested");
            return new ResponseResult(permissions.Values);
        }


    }
}
