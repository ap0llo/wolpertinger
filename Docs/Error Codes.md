Error Codes
========

General Errors
---------------
Errors not related to any particular category

<table>
    <tr>
        <th>Error Code</th>
        <th>Error Name</th>
        <th>Description</th>
    </tr>    
    <tr>
	    <td>100</td>
	    <td>Unspecified Error</td>
	    <td>Not specified error</td>
    </tr>
</table>

Security Errors
---------------
Errors related to application security
    
<table>
    <tr>
        <th>Error Code</th>
        <th>Error Name</th>
        <th>Description</th>
    </tr>   
    <tr>
	    <td>200</td>
	    <td>SecurityError</td>
	    <td>Not specified security error</td>
    </tr>
    <tr>
	    <td>201</td>
	    <td>NotAuthorizedError</td>
	    <td>The caller is not authorized to make his method-call (see Authentication, Trust-Levels) </td>
    </tr>
    <tr>
	    <td>202</td>
	    <td>EncryptionError</td>
	    <td>A message that was expected to be encrypted was not encrypted or was encrypted using a wrong key</td>
    </tr>
    <tr>
	    <td>203</td>
	    <td>InvalidSignatureError</td>
	    <td>A signed message's signature could not be verified</td>
    </tr>
</table>


Request Errors
---------------
Errors related to requests/method calls from other clients
<table>
    <tr>
        <th>Error Code</th>
        <th>Error Name</th>
        <th>Description</th>
    </tr>   
    <tr>
	    <td>300</td>
	    <td>RequestError</td>
	    <td>Not specified request error</td>
    </tr>
    <tr>
	    <td>301</td>
	    <td>InvalidXmlError</td>
	    <td>The received data wasn’t valid XML or could not be validated againt the xml schema</td>
    </tr>
    <tr>
	    <td>302</td>
	    <td>UnknownMessage</td>
	    <td>The received message was not understood or is not supported.</td>
    </tr>
    <tr>
	    <td>303</td>
	    <td>MethodNotFoundError</td>
	    <td>The sender tried to call a method that coud not be found.</td>
    </tr>
    <tr>
        <td>304</td>
        <td>ComponentNotFound</td>
        <td>The component specified in the targetname of a request could not be found</td>
    </tr>
    <tr>
	    <td>305</td>
	    <td>InvalidParametersError</td>
	    <td>The sender specified invalid (e.g. wrong type, missing paramters) for the method-call. </td>
    </tr>
    <tr>
	    <td>306</td>
	    <td>UnsupportedDatatypeError</td>
	    <td>An object of a type that is not supported in the protocol has been 
	    encountered </td>
    </tr>
</table>


Response Errors
-----------------
Errors related to repsonse messages received by another client
<table>
    <tr>
        <th>Error Code</th>
        <th>Error Name</th>
        <th>Description</th>
    </tr>
    <tr>
	    <td>400</td>
	    <td>ResponeError</td>
	    <td>Not specified response error</td>
    </tr>
    <tr>
	    <td>401</td>
	    <td>InvalidResponseError</td>
	    <td>A response received that was no valid</td>
    </tr>
</table>

Fileserver Errors
----------------
Errors related to the Fileserver component
<table>
    <tr>
        <th>Error Code</th>
        <th>Error Name</th>
        <th>Description</th>
    </tr>   
    <tr>
	    <td>500</td>
	    <td>FileserverError</td>
	    <td>Not specified Fileserver error</td>
    </tr>
    <tr>
	    <td>501</td>
	    <td>MountError</td>
	    <td>A shared folder could no be mounted at it&#39;s location in the virtual filesystem</td>
    </tr>
    <tr>
	    <td>502</td>
	    <td>ItemNotFoundError</td>
	    <td>The calling client tried to access an item that could not be located</td>
    </tr>
</table>
