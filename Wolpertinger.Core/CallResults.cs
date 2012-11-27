/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Base class for results of server-component methods
    /// </summary>
    public abstract class CallResult
    {
        /// <summary>
        /// A action that will be executed after the CallResult has been process by the ClientConnection (optional)
        /// </summary>
        public Action PostProcessingAction { get; set; }
    }

    /// <summary>
    /// A CallResult that indicates that a call was completed successfully but
    /// no should be sent back to the calling client
    /// </summary>
    public class VoidResult : CallResult
    { }

    /// <summary>
    /// A CallResult that indicates that an error has occurred and a RemoteError should be sent to the calling client
    /// </summary>
    public class ErrorResult : CallResult
    {
        /// <summary>
        /// The error-code of the error that occurred
        /// </summary>
        public RemoteErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance or ErrorResult with the specified error-code
        /// </summary>
        /// <param name="errorCode">The error-code of the error that occurred</param>
        public ErrorResult(RemoteErrorCode errorCode)
        {
            this.ErrorCode = errorCode;
        }
    }



    /// <summary>
    /// A CallResult that indicates that the call was completed successfully and 
    /// the encapsulated value should be sent back to the calling client as response.
    /// </summary>
    public class ResponseResult : CallResult
    {
        /// <summary>
        /// The value that will be sent back to the calling client
        /// </summary>
        public object ResponseValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the ResponseResult
        /// </summary>
        public ResponseResult() { }

        /// <summary>
        /// Initializes a new instance of the ResponseResult with the specified response value
        /// </summary>
        /// <param name="value">The value that will be sent back to the calling client</param>
        public ResponseResult(object value)
        {
            this.ResponseValue = value;
        }


    }
}
