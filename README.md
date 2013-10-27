# DispatchQueue

C# dispatch queue in the spirit of Grand Central Dispatch (GCD) and HawtDispatch.  A queue holds C# Actions which are run on a worker thread with the guarantee that it will only be active on one thread at a time.

## Example Code

```csharp
// create a thread pool based dispatch
ThreadPoolDispatcher dispatcher = new ThreadPoolDispatcher();

// create the queue from the dispatcher
IActionQueue queue = dispatcher.CreateQueue();

// queue an Action for processing 
queue.Enqueue(() => { Console.Out.WriteLine ("On worker thread"); });

```
