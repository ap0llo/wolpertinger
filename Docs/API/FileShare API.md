FileShare
=========

FileShare exposes an API for both managing shared directories (shares, mountpoints, permissons etc.) and accessing informaition about shared directories and files.

## Members

<table>
    <tr>
        <th>Name</th>
        <th>Required Trust Level</th>
        <th>Parameters</th>
        <th>Returns</th>
        <th>Description</th>
    </tr>
    <tr>
        <td>AddSharedDirectory</td>
        <td>4</td>
        <td>
          <code>string internalPath</code><br />
          <code>string virtualPath</code><br />
        </td>
        <td><code>void</code></td>
        <td>Shares the specified local path on the target-machine and mounts it at the specified virtual directory.</td>
    </tr>
    <tr>
        <td>GetDirectoryInfo</td>
        <td>3</td>
        <td>
          <code>string virtualPath</code><br />
          <code>int32 depth</code><br />
        </td>
        <td><code>Wolpertinger.DirectoryObject</code></td>
        <td>Returns information about the directory at the specified path and its files. The &#39;depth&#39; paramter specifies how many levels of sub-directories are to be included in the response. To include everything, set depth to -1. Returning everything should be used with caution, as it might take the called client long to respond to the request and, more importantly cause a very big response-message that might take long to transmit.</td>
    </tr>
    <tr>
        <td>GetFileInfo</td>
        <td>3</td>
        <td><code>string virtualPath</code></td>
        <td><code>Wolpertinger.FileObject</code></td>
        <td>Returns a FileObject object with information about the file at the specified path.</td>
    </tr>
    <tr>
        <td>GetMounts</td>
        <td>4</td>
        <td>none</td>
        <td><code>List&lt;Wolpertinger.MountInfo&gt;</code></td>
        <td>Returns a list with information about all explicitly added shared directories.</td>
    </tr>
    <tr>
        <td>AddPermission</td>
        <td>4</td>
        <td><code>Wolpertinger.Permission p</code></td>
        <td><code>void</code></td>
        <td>Adds a new Permisson. Note that there can only be one Permission set for a path. To modify a existing permission, it needs to be removed and re-added afterwards.</td>
    </tr>
    <tr>
        <td>GetPermission</td>
        <td>3</td>
        <td><code>string path</code></td>
        <td><code>boolean</code></td>
        <td>Returns a boolean value indicating whether the calling client is permitted to access the specified path.</td>
    </tr>
    <tr>
        <td>GetAddedPermissions</td>
        <td>4</td>
        <td>none</td>
        <td><code>List&lt;Wolpertinger.Permission&gt;</code></td>
        <td>Gets a list of all explicitly added Permissions.</td>
    </tr>
    <tr>
        <td>RemovePermission</td>
        <td>4</td>
        <td><code>string path</code><br /></td>
        <td><code>void</code></td>
        <td>Removes a explicitly added permission for the specified path.</td>
    </tr>
    <tr>
        <td>SetRootDirectoryPath</td>
        <td>4</td>
        <td><code>string internalPath</code><br /></td>
        <td><code>void</code></td>
        <td>Sets the local path on the target-machine of the folder that will be used as root-directory.</td>
    </tr>
    <tr>
        <td>GetRootDirectoryPath</td>
        <td>4</td>
        <td>none</td>
        <td><code>string</code></td>
        <td>Returns the local path on the target machine of the folder that is currently used as root-directory. If no root-directory has been set, the return-value will be an empty string.</td>
    </tr>
	<tr>
		<td>GetSnapshots</td>
		<td>3</td>
		<td>none</td>
		<td><code>List&lt;SnapshotInfo&gt;</code></td>
		<td>Returns a lsit of all available filesystem snapshot</td>
	</tr>
	<tr>
		<td>CreateSnapshot</td>
		<td>3</td>
		<td>none</td>
		<td><code>Guid</code></td>
		<td>Creates a new filesystem snapshot and returns its Id</td>
	</tr>
</table>
