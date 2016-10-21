# Quickbird UWP App

This doc is the what where why of this app.

## Applications Startup Flow

Entry point: `App.xaml.cs`, `OnLaunched()` is where most the initialisation happens.

The first view is spawned from `OnVisibilityChanged()`, it chooses `SyncingView` or `LandingPage` depending if the `IsLoggedIn` setting is set.

The network code doesn't start until the navigation to the `ShellView`, `StartSession()` is called externally from `Shell.xaml.cs`. 

```
  +---------------------------------------------------------------------------+
  |                                                                           |
  |  App.Xaml.cs                            +------------------------------+  |
  |                                         |                              |  |
  |   +----------------------------------+  | StartSession()               |  |
  |   |                                  |  | :Start websocket             |  |
  |   | OnLaunched()                     |  | :Start GnatMQ                |  |
  |   | :RootFrame created.              |  |                              |  |
  |   | :OnVisibilityChanged() subbed.   |  +---------------^--------------+  |
  |   |                                  |                  |                 |
  |   +----------------+-----------------+  +---------------------------------+
  |                    |                    |               |
  |   +----------------v-----------------+  |     +---------+-----------------+
  |   |                                  |  |     |                           |
  |   | OnVisibilityChanged()            |  |     | ShellView                 |
  |   | :Navigates RootFrame             |  |     |                           |
  |   |                                  |  |     +--------------^------------+
  |   +-------------------------+--------+  |                    |
  |                             |           |                    |
  +-----------------------------------------+                    |
                                |                                |
         Not logged in          |      Logged in                 |
     +--------------------------+-----------------------+        |
     |                                                  |        |
     |                                                  |        |
+----v-------------------+        +---------------------v--------+------------+
|                        |        |                                           |
| LandingPage            +--------> SyncingView                               |
|                        |        |                                           |
+----^-------------------+        +-+---------------------------------------^-+
     |                              |                                       |
     |                              | Roaming creds changed (signs in again)|
     |                              |                                       |
     |                            +-v---------------------------------------+-+
     |   DontSignBackInAgain      |                                           |
     +----------------------------+ SignOutView                               |
                                  |                                           |
                                  +-------------------------------------------+


```