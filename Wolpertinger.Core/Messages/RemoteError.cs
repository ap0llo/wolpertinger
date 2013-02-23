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
using System.Xml.Linq;
using Nerdcave.Common.Extensions;
using Slf;
using Nerdcave.Common.Xml;

namespace Wolpertinger.Core
{
	[XmlTypeName("Wolpertinger.RemoteError")]
	public class RemoteError : RpcMessage
	{

		private RemoteErrorCode errorCode;

		/// <summary>
		/// The error-code of the error that occurred
		/// </summary>
		public RemoteErrorCode ErrorCode 
		{
			get { return errorCode; }
			set
			{
				errorCode = value;
			}
		}

		/// <summary>
		/// The Id of the remote method call that caused the error
		/// </summary>
		public Guid CallId { get; set; }


		public RemoteError() : this(RemoteErrorCode.UnspecifiedError) 
		{ }

		public RemoteError(RemoteErrorCode errorCode)
		{
			this.CallId = Guid.Empty;
			this.ErrorCode = errorCode;
		}


		#region ISerializable Members
		
		public override XElement Serialize()
		{
			XElement xml = new XElement("RemoteError");
			xml.Add(new XElement("TargetName") { Value = this.ComponentName });
			xml.Add(new XElement("ErrorCode") { Value = ((int)this.ErrorCode).ToString() });

			if (this.CallId != Guid.Empty) 
				xml.Add(new XElement("CallId") { Value = this.CallId.ToString() });

			return xml;
		}


		public override void Deserialize(XElement xmlData)
		{
			ComponentName = xmlData.Element("TargetName").Value;
			try
			{
				ErrorCode = (RemoteErrorCode)Int32.Parse(xmlData.Element("ErrorCode").Value);				
			}
			catch (FormatException fex)
			{
				LoggerService.GetLogger("RemoteError").Error(fex);
				ErrorCode = 0;
			}
			if(xmlData.Elements("CallId").Any())
			{
                CallId = XmlSerializer.DeserializeAs<Guid>(xmlData.Element("CallId"));
			}
		
		}

		#endregion
	}


	public enum RemoteErrorCode
	{
		//General Errors
		UnspecifiedError = 100,
		//Security Errors
		SecurityError = 200,
		NotAuthorizedError = 201,
		EncryptionError = 202,
		InvalidSignature = 203,
		//Request Errors
		RequestError = 300,
		InvalidXmlError = 301,
		UnknownMessage = 302,
		MethodNotFoundError = 303,
		ComponentNotFoundError = 304,
		InvalidParametersError = 305,
		UnsupportedDatatypeError = 306,
		//Response Errors
		ResponseError = 400,
		InvalidResponseError =401,
		//Fileserver Errors
		FileserverError = 500,
		MountError = 501,
		ItemNotFoundError = 502
	}

}
