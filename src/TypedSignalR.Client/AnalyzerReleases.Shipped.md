## Release 3.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
TSRC000 |  Usage   |  Error   | Unexpected exception.
TSRC001 |  Usage   |  Error   | The type argument must be an interface.
TSRC002 |  Usage   |  Error   | Only method definitions are allowed in the interface.
TSRC003 |  Usage   |  Error   | The return type of methods in the interface used for hub proxy must be Task, Task<T>, IAsyncEnumerable<T>, Task<IAsyncEnumerable<T>> or Task<ChannelReader<T>>.
TSRC004 |  Usage   |  Error   | The return type of methods in the interface used for receiver must be Task or Task<T>.
TSRC005 |  Usage   |  Error   | CancellationToken can be used as a parameter only in the server-to-client streaming method.
TSRC006 |  Usage   |  Error   | Using multiple CancellationToken in method parameters is prohibited.
TSRC007 |  Usage   |  Error   | Using IAsyncEnumerable<T> or ChannelReader<T> as a parameter in a server-to-client streaming method is prohibited.
TSRC008 |  Usage   |  Error   | The return type of client-to-server streaming method must be Task.
