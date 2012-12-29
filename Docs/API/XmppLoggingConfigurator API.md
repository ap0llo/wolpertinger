XmppLoggingConfigurator
=======================

Wolpertinger includes a logger, that can automatically send log-events to any Jabber-Account (preferably someone who can solve problems that may have occurred).
The XmppLoggingConfigurator component provides an API to configure this logger.


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
        <td>SetEnable</td>
        <td>4</td>
        <td><code>boolean enable</code></td>
        <td><code>void</code></td>
        <td>Enables/Disables the XmppLogger</td>
    </tr>
    <tr>
        <td>GetEnable</td>
        <td>4</td>
        <td>none</td>
        <td><code>boolean</code></td>
        <td>Returns if the XmppLogger is enabled or not</td>
    </tr>
    <tr>
        <td>SetRecipient</td>
        <td>4</td>
        <td><code>string recipient</code></td>
        <td><code>void</code></td>
        <td>Sets the recipient for log-messages</td>
    </tr>
    <tr>
        <td>GetRecipient</td>
        <td>4</td>
        <td>none</td>
        <td><code>string</code></td>
        <td>Gets the currently set recipient for log-messages</td>
    </tr>
    <tr>
        <td>SetLogLevel</td>
        <td>4</td>
        <td><code>string loglevel</code></td>
        <td><code>void</code></td>
        <td>Sets the minimum level of events that will be sent by XmppLogger</td>
    </tr>
    <tr>
        <td>GetLogLevel</td>
        <td>4</td>
        <td>none</td>
        <td><code>string</code></td>
        <td>Gets the currently set log-level.</td>
    </tr>
    <tr>
        <td>SetEnableDebugLogging</td>
        <td>4</td>
        <td>
          <code>boolean enable</code><br />
        </td>
        <td><code>void</code></td>
        <td>Enables/Disables sending of &quot;Debug&quot; log-events</td>
    </tr>
    <tr>
        <td>GetEnableDebugLogging</td>
        <td>4</td>
        <td>none</td>
        <td><code>boolean</code></td>
        <td>Returns if sending of &quot;Debug&quot; log-events is enabled</td>
    </tr>
    <tr>
        <td>TestLogging</td>
        <td>4</td>
        <td><code>string loglevel</code></td>
        <td><code>void</code></td>
        <td>Creates a &quot;Test&quot; log messages with the indcated log-level</td>
    </tr>
</table>
