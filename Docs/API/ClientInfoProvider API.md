ClientInfoProvider
==================

The ClientInfo component implements methods for exchange of information about the client itself and other clients.

##Members

<table>
    <tr>
        <th>Name</th>
        <th>Required Trust Level</th>
        <th>Parameters</th>
        <th>Returns</th>
        <th>Description</th>
    </tr>
    <tr>
        <td>GetClientInfo</td>
        <td>3</td>
        <td>none</td>
        <td><code>ClientInfo</code></td>
        <td>Returns a ClientInfo object containing information the called client has about the calling client.</td>
    </tr>
    <tr>
        <td>GetKnownClients</td>
        <td>3</td>
        <td>none</td>
        <td><code>List&lt;ClientInfo&gt;</code></td>
        <td>Returns all wolpertinger clients known to the called instance.</td>
    </tr>
</table>

