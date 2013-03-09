Reference Implementation
================
This page describes the refereence implementation of wolpertinger. It is buildt in C# and currently targets .NET 4.0

Architecture Overview 
----------------------
![Architecture Overview](Architecture.png)
        
As you can see in the above diagram, the implementation consists of 4 main components and 2 factories used by them. All of these components are loosely coupled via interfaces. 

- **ConnectionManager (<code>IConnectionManager</code>)**: Manages all ClientConnections and one or more messaging-client
- **MessagingClient (<code>IMessagingClient</code>)**: Messaging-clients implement the actual communication with other systems. For example, Xmpp messaging is implemented as a IMessagingClient
- **WtlpClient (<code>IWtlpClient</code>)**: IWtlpClient encapsulates implementations of layer 1 (*wtlp = wolpertinger transmission layer protocol*) of the communication protocol. An instance of a wtlp-client is responsible for communication with exactly one peer. To actually implement message-sending and -receiving it uses a messaging-client. It processes every message from the client it is responsible for and fire's its MessageReceived event accordingly
- **ClientConnection (<code>IClientConnection</code>)**: ClientConnection implements the second layer of the communication protocol. It is used to call methods on other systems and handles incoming calls
- **ConnectionFactory (<code>IConnectionFactory</code>)**: The connection-factory is used by the connection-manager to instantiate new client-connections
- **ComponentFactory (<code>IComponentFactory</code>)** is used by ClientConnections to instantiate new ServerComponents


Components 
-----------
There are two types of components

- ServerComponent
- ClientComponent 

ServerComponents are the more important of them. All methods that can be called from other clients are implemented in such components. How such a component is implemented is described below.  
ClientComponents were more important in earlier versions. Now they are just used to encapsulate calling methods of a server-component on the caller's side. In most cases they just call the <code>CallRemoteAction()</code> or <code>CalRemoteFunction()</code> of a client-connection. 



Default Implementations
--------------------------

###DefaultConnectionManager
implements <code>IConnectionManager</code>
    
Default implementation of a ConnectionManager. It, at the moment, uses a single instance of XmppMessagingClient to send and receive messages.  
It also manages all active client-connections.


###DefaultComponentFactory
implements <code>IComponentFactory</code>

Component factories are used to instantiate server-components to respond to incoing RemoteMethodCalls.

When the application starts, DefaultComponentFactory loads all types from all loaded assemblies that implement the <code>IComponent</code> interface.  
For all of these types it checks the <code>ComponentAttribute</code> attribute to determine the component's name. When a component for a certain component-name is requested, it will look up the required type in a dictionary and return a new instance of that type.
    
###DefaultConnectionFactory
implements <code>IConnectionFactory</code>

Connection-factories are used by the connection-manager for instantiating new client-connections.

This implemenation is pretty simple and will return a new instance of DefaultClientConnection for every request.
    

###DefaultClientConnection
implements <code>IClientConnection</code>

DefaultClientConnection uses a component-factory to initialize server-components and tries to handle incoming messages accordingly.  
Once as server-component has been initialized, it will be kept alive for future use, so that components do not need to be stateless and can save temporary data in local variables.

To determine which methods to call in a component when a RemoteMethodCall is received, it checks every of the compnonent's method for the <code>MethodCallHandler</code> attribute which is used to specify which remote-methods a method handles.  
ClientConnection also enforces the security policy and checks if the calling client is permitted to invoke a method or not.

For details on the security model, see [Authentication](authentication-process)



Implementing Server Components
----------------------------

A server-component is quite simple to implement.

It needs to implement the <code>IComponet</code> interface to be able to interact with the ClientConnection it is hosted by. For the class, you also need to add a ComponentAttribute so that the component factory can associate the component with a component-name.  

For every remote method you want to handle in the component, you need to add a method that returns a <code>CallResult</code>. The parameters of the method need to match the number, order and type of the RemoteMethod as they will be passed to the method "as they come in" by the ClientConnection.
        
The method can return one of the following sub-classes of <code>CallResult</code>

-	**ResponseResult** to make ClientConnection respond to the method call with the specified value
-	**VoidResult** to make ClientConnection do nothing
-	**ErrorResult** to make ClientConnection send the caller a RemoteError
        
In order to determine which method handles which RemoteMethodCall, each method has to havea <code>MethodCallHandler</code> attribute specifying the RemoteMethod's name.  
Which method is called by ClientConnection is determined solely by this attribute, so the method's name does not have to follow any naming conventions.

Every method should also specify the trust level a client needs to be allowed to call the method. This can be done using the <code>TrustLevel</code> attribute.

If no trust level is specified, <code>TrustLevel.MAX</code> will be assumed which will fail in most cases.

####Sample
This is a sample of a very simple server-component with only one method. As you can see, the componet's name is "Sample" and handles calls to the "Test" remote-method. Trust-Level 2 is required to call the "Test"
        
	[Component("Sample")]
	public class SampleServerComponent : IComponent
	{
    		[MethodCallHandler("Test"), TrustLevel(2)]
    		public CallResult Test()
    		{
        		return new ResponseResult("This is the string the method returns");
			}

    		//ClientConnection as defined in IComponent
    		public IClientConnection ClientConnection { get; set; }

	}
   
