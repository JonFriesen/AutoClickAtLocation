AutoClickAtLocation
===================

The backstory to this quick little program is over a conversation with a friend whos been playing the 1995 classic
Command & Conquer Red Alert. He was talking about how building units does not allow for queueing and he found it
annoying that to build a sizeable army he'd have to be constantly clicking. Thus I ripped out this little program.

Usage:

When the user presses Ctrl+F the cursor location will be noted and after the user set delay, this happens repeatedly
until the user presses Ctrl+G.

How it Works:

Using a basic GUI on key event (monitored by P/Invoke keyboard hook) the VirtualKeyboard class reaches out and sets
some static variables then starts an abandoned thread which reaches out and grabs the static variables that were set.

The importances of the thread is to maintain the clicking, while leaving the original UI thread monitoring for the
key shortcut to stop clicking.

On stop event the VirtualKeyboard class will set a static boolean which is read by the threaded click loop on every
iteration. When the loop is broken the thread finishes and is cleaned by the CLR garbage collection.
