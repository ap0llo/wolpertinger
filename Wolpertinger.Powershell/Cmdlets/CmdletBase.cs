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
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;

namespace Wolpertinger.Powershell.Cmdlets
{
	public abstract class CmdletBase
		: PSCmdlet
	{
		[Parameter(Position = 1, Mandatory = true, ParameterSetName = ParameterSets.FromConnection)]
		public IClientConnection Connection{ get; set; }




		protected override void ProcessRecord()
		{
			try
			{
				processRecordImplementation();
			}
			catch (AggregateException ex)
			{
				handleException(ex);                
			}
			catch (TimeoutException ex)
			{
				handleException(ex);                                
			}
			catch (RemoteErrorException ex)
			{
				handleException(ex);                                
			}            
		}




		protected virtual void processRecordImplementation()
		{
			//no logic in base implementation
		}



		private bool canHandleException(Exception ex)
		{
			return (ex is AggregateException || ex is TimeoutException || ex is RemoteErrorException);
		}

		private void handleException(Exception ex)
		{
			if (ex is AggregateException)
			{
				var aggregateException = ex as AggregateException;
				aggregateException.Flatten();

				foreach (var innerException in aggregateException.InnerExceptions)
				{
					if(canHandleException(innerException))
					{
						handleException(innerException);
					}
					else
					{
						throw innerException;
					}
				}
			}
			else if (ex is RemoteErrorException)
			{
				var remoteErrorException = ex as RemoteErrorException;
				var errorRecord = new ErrorRecord(ex, "RemoteError: " + remoteErrorException.Error.ToString(), ErrorCategory.NotSpecified, Connection);
				WriteError(errorRecord);
			}
			else
			{
				var errorRecord = new ErrorRecord(ex, "Timeout", ErrorCategory.ConnectionError, Connection);
				WriteError(errorRecord);
			}
		}

	}
}
